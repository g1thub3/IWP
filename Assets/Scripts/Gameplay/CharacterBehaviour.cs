using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ATTACK_TYPE
{
    NEUTRAL,
    PHYSICAL,
    MAGIC
}

public class CharacterBehaviour : MonoBehaviour
{

    DungeonUIHandler _dungeonUI;
    DGEntity _entity;
    DGGameManager _dgGameManager;
    DGGenerator _dungeonGen;

    public Transform allianceIndicator;
    [SerializeField] GameObject _dmgIndicator;
    public AudioSource personalSource;

    public CharacterEntry character;
    public int alliance;

    public int health;
    public int hunger;

    public int energy;
    public int mana;

    public DefaultAttack defaultAttackInstance;
    private List<CombatMove> _availableMoves;

    public void SetUp(CharacterEntry characterData)
    {
        character = characterData;
        character.ApplyCharacter();
        character.Recalculate();

        health = character.maxHealth.CurrStat;
        hunger = character.hungerSize.CurrStat;

        energy = character.maxEnergy.CurrStat;
        mana = character.maxMana.CurrStat;

        _availableMoves = new List<CombatMove>();
        for (int i = 0; i < character.Profile.availableMoves.Count; i++) { 
            _availableMoves.Add(character.Profile.availableMoves[i]);
        }

        List<int> scores = new List<int>(_availableMoves.Count); // Sort by the best moves to use, then select them based on whether they'll succeed
        for (int i = 0; i < _availableMoves.Count; i++)
        {
            scores.Add(0);
        }
        for (int i = 0; i < _availableMoves.Count; i++)
        {
            scores[i] -= _availableMoves[i].energyRequirement;
            if (_availableMoves[i] is AttackMove)
            {
                var move = _availableMoves[i] as AttackMove;
                int dmg = move.baseDamage;
                if (move.moveType == MOVE_TYPE.PHYSICAL)
                {
                    dmg += character.physAtk.CurrStat;
                } else if (move.moveType == MOVE_TYPE.MAGICAL)
                {
                    dmg += character.magicAtk.CurrStat;
                }
                scores[i] += dmg;
            }
        }
        for (int i = 0; i < _availableMoves.Count; i++)
        {
            for (int j = i; j < _availableMoves.Count; j++)
            {
                if (scores[i] < scores[j])
                {
                    var temp1 = scores[j];
                    var temp2 = _availableMoves[j];
                    scores[j] = scores[i];
                    _availableMoves[j] = _availableMoves[i];
                    scores[i] = temp1;
                    _availableMoves[i] = temp2;
                }
            }
        }
        _availableMoves.Add(defaultAttackInstance);
    }

    public void Damage(int baseDamage, ATTACK_TYPE atkType, CharacterBehaviour attacker = null)
    {
        int finalValue = baseDamage;
        if (atkType == ATTACK_TYPE.PHYSICAL)
        {
            finalValue = Mathf.Max(1, finalValue + attacker.character.physAtk.CurrStat - character.physDef.CurrStat);
        } else if (atkType == ATTACK_TYPE.MAGIC)
        {
            finalValue = Mathf.Max(1, finalValue + attacker.character.magicAtk.CurrStat - character.magicDef.CurrStat);
        }

        var og = health;
        health = Mathf.Clamp(health - finalValue, 0, character.maxHealth.CurrStat);
        var diff = og - health;
        if (diff > 0)
        {
            _dungeonUI.AddEntry(gameObject.name + " took " + diff + " damage!");
            var indicator = Instantiate(_dmgIndicator);
            indicator.transform.position = TileInfo.CoordToPosition(_entity.Position);
            indicator.GetComponent<DamageIndicator>().text.text = "-" + diff;

            VFXManager.Instance.Create("hit_effect", TileInfo.CoordToPosition(_entity.Position));
            AudioManager.Instance.PlayFromObject(personalSource, "Damage");

            if (atkType == ATTACK_TYPE.PHYSICAL)
            {
                indicator.GetComponent<DamageIndicator>().text.color = new Color(1.0f, 0.68f, 0.41f);
            }
            else if (atkType == ATTACK_TYPE.MAGIC)
            {
                indicator.GetComponent<DamageIndicator>().text.color = new Color(0.73f, 0.61f, 0.88f);
            }
            if (TryGetComponent<DGPlayer>(out DGPlayer plr))
            {
                plr.OnLeaderStatChanged.Invoke(CHARACTER_STAT.HEALTH, health, character.maxHealth.CurrStat);
            }
        }
        if (health <= 0)
        {
            _dgGameManager.RegisterDead(this, attacker);
        }
        if (_dungeonGen.ActiveParty.Contains(this))
            _dungeonUI.UpdatePartyStatus(this);
    }

    public void Replenish(int amount, CHARACTER_STAT toReplenish)
    {
        if (amount < 0 && toReplenish == CHARACTER_STAT.HEALTH)
        {
            Damage(amount * -1, ATTACK_TYPE.NEUTRAL);
            return;
        }
        int og, diff;
        switch (toReplenish)
        {
            case CHARACTER_STAT.HEALTH:
                og = health;
                health = Mathf.Clamp(health + amount, 0, character.maxHealth.CurrStat);
                diff = health - og;
                if (diff > 0)
                {
                    _dungeonUI.AddEntry(gameObject.name + " healed for " + diff + " HP!");
                    if (TryGetComponent<DGPlayer>(out DGPlayer plr))
                    {
                        plr.OnLeaderStatChanged.Invoke(toReplenish, health, character.maxHealth.CurrStat);
                    }
                } else
                {
                    _dungeonUI.AddEntry("But nothing changed!");
                }
                break;
            case CHARACTER_STAT.ENERGY:
                og = energy;
                energy = Mathf.Clamp(energy + amount, 0, character.maxEnergy.CurrStat);
                diff = energy - og;
                if (diff > 0)
                {
                    _dungeonUI.AddEntry(gameObject.name + " regained " + diff + " energy!");
                    if (TryGetComponent<DGPlayer>(out DGPlayer plr))
                    {
                        plr.OnLeaderStatChanged.Invoke(toReplenish, energy, character.maxEnergy.CurrStat);
                    }
                }
                else
                {
                    _dungeonUI.AddEntry("But nothing changed!");
                }
                break;
            case CHARACTER_STAT.MANA:
                og = mana;
                mana = Mathf.Clamp(mana + amount, 0, character.maxMana.CurrStat);
                diff = mana - og;
                if (diff > 0)
                {
                    _dungeonUI.AddEntry(gameObject.name + " regained " + diff + " mana!");
                    if (TryGetComponent<DGPlayer>(out DGPlayer plr))
                    {
                        plr.OnLeaderStatChanged.Invoke(toReplenish, mana, character.maxMana.CurrStat);
                    }
                }
                else
                {
                    _dungeonUI.AddEntry("But nothing changed!");
                }
                break;
            case CHARACTER_STAT.HUNGER:
                og = hunger;
                hunger = Mathf.Clamp(hunger + amount, 0, character.hungerSize.CurrStat);
                diff = hunger - og;
                if (diff > 0)
                {
                    _dungeonUI.AddEntry(gameObject.name + " regained " + diff + " hunger!");
                    if (TryGetComponent<DGPlayer>(out DGPlayer plr))
                    {
                        plr.OnLeaderStatChanged.Invoke(toReplenish, hunger, character.hungerSize.CurrStat);
                    }
                }
                else
                {
                    _dungeonUI.AddEntry("But nothing changed!");
                }
                break;
        }
        if (_dungeonGen.ActiveParty.Contains(this))
            _dungeonUI.UpdatePartyStatus(this);
    }
    public void Consume(int amount, CHARACTER_STAT toConsume)
    {
        switch (toConsume)
        {
            case CHARACTER_STAT.ENERGY:
                energy = Mathf.Clamp(energy - amount, 0, character.maxEnergy.CurrStat);
                if (TryGetComponent<DGPlayer>(out DGPlayer plr))
                {
                    plr.OnLeaderStatChanged.Invoke(CHARACTER_STAT.ENERGY, energy, character.maxEnergy.CurrStat);
                }
                break;
            case CHARACTER_STAT.MANA:
                mana = Mathf.Clamp(mana - amount, 0, character.maxMana.CurrStat);
                if (TryGetComponent<DGPlayer>(out DGPlayer plr2))
                {
                    plr2.OnLeaderStatChanged.Invoke(CHARACTER_STAT.MANA, mana, character.maxMana.CurrStat);
                }
                break;
        }
        if (_dungeonGen.ActiveParty.Contains(this))
            _dungeonUI.UpdatePartyStatus(this);
    }

    public CharacterBehaviour HitDetect(TileCoord position)
    {
        TileInfo detectedTile = _entity.Floor.tiles[_entity.Floor.CoordToIndex(position)];
        DGEntity hit = detectedTile.occupyingEntity;
        if (hit != null)
        {
            var target = hit.GetComponent<CharacterBehaviour>();
            if (target.alliance != alliance && target.alliance != -1)
            {
                return target;
            }
        }
        return null;
    }

    public bool DropItem()
    {
        if (character.HeldItem == null || character.HeldItem.module == null)
            return false;
        Item toDrop = character.HeldItem;
        TileInfo available = _dungeonGen.SearchNextAvailableTile(_entity.Floor.CoordToTileInfo(_entity.Position), SearchConditions.New(false, false), 0);
        if (available != null)
        {
            var itemContainer = Tilesets.Instance.ConstructItemInteractable(toDrop);
            _dungeonGen.InsertItem(itemContainer, available);
            character.HeldItem = null;
            _dungeonUI.AddEntry(gameObject.name + " has dropped a " + toDrop.module.itemName + "!");
            _dgGameManager.OnItemDropped(itemContainer);
            return true;
        }
        return false;
    }
    public bool DropItem(int index)
    {
        if (index < 0 || index >= GlobalGameManager.Instance.inventory.Count)
            return false;
        Item toDrop = GlobalGameManager.Instance.inventory[index];
        TileInfo available = _dungeonGen.SearchNextAvailableTile(_entity.Floor.CoordToTileInfo(_entity.Position), SearchConditions.New(false, false), 0);
        if (available != null)
        {
            var itemContainer = Tilesets.Instance.ConstructItemInteractable(toDrop);
            _dungeonGen.InsertItem(itemContainer, available);
            GlobalGameManager.Instance.inventory.RemoveAt(index);
            _dungeonUI.AddEntry(gameObject.name + " has dropped a " + toDrop.module.itemName + "!");
            return true;
        }
        return false;
    }

    public void OnTurnComplete()
    {
        if (hunger > 0)
        {
            hunger--;
            if (TryGetComponent<DGPlayer>(out DGPlayer plr))
            {
                plr.OnLeaderStatChanged.Invoke(CHARACTER_STAT.HUNGER, hunger, character.hungerSize.CurrStat);
            }
        } else
        {
            if (health > 1)
            {
                health--;
                if (TryGetComponent<DGPlayer>(out DGPlayer plr))
                {
                    plr.OnLeaderStatChanged.Invoke(CHARACTER_STAT.HEALTH, health, character.maxHealth.CurrStat);
                }
            }
        }
        if (energy < character.maxEnergy.CurrStat)
        {
            energy++;
            if (TryGetComponent<DGPlayer>(out DGPlayer plr))
            {
                plr.OnLeaderStatChanged.Invoke(CHARACTER_STAT.ENERGY, energy, character.maxEnergy.CurrStat);
            }
        }
        if (_dungeonGen.ActiveParty.Contains(this))
            _dungeonUI.UpdatePartyStatus(this);
    }

    public string GetDescription()
    {
        StringBuilder description = new StringBuilder();
        description.AppendLine("Level: " + character.characterLevel);
        description.AppendLine("EXP: " + character.experiencePoints + " / " + character.ExpToNextLevel);
        if (character.HeldItem == null)
        {
            description.AppendLine("Held Item: None");
        }
        else
        {
            description.AppendLine("Held Item: " + character.HeldItem.ToString());
        }
        description.Append("\n");
        description.AppendLine("HP: " + health + " / " + character.maxHealth.CurrStat + " | Hunger: " + hunger + " / " + character.hungerSize.CurrStat);
        description.AppendLine("Energy: " + energy + " / " + character.maxEnergy.CurrStat + " | Mana: " + mana + " / " + character.maxMana.CurrStat);
        description.Append("\n");
        description.AppendLine("PA: " + character.physAtk.CurrStat + " | PD: " + character.physDef.CurrStat);
        description.AppendLine("MA: " + character.magicAtk.CurrStat + " | MD: " + character.magicDef.CurrStat);

        return description.ToString();
    }

    public CombatMove SelectMove()
    {
        for (int i = 0; i < _availableMoves.Count; i++)
        {
            bool canPerform = _availableMoves[i].CanBePerformed(this);
            bool willSucceed = _availableMoves[i].WillMoveSucceed(this);
            if (canPerform && willSucceed)
            {
                return _availableMoves[i];
            }
        }
        return null;
    }

    public bool PerformMove(CombatMove move)
    {
        if (!move.CanBePerformed(this)) return false;
        move.Perform(this);
        return true;
    }
    private void Start()
    {
        personalSource = GetComponent<AudioSource>();
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _entity = GetComponent<DGEntity>();
        _dgGameManager = FindAnyObjectByType<DGGameManager>();
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
        _dungeonUI.UpdateLeaderInfo();
    }

    private void Update()
    {
        if (allianceIndicator != null)
        {
            allianceIndicator.Rotate(new Vector3(0, 0, 5 * Time.deltaTime));
        }
    }

}
