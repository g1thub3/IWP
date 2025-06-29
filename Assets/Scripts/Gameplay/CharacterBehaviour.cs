using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

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

    [SerializeField] GameObject _dmgIndicator;

    public CharacterEntry character;
    public int alliance;

    public int health;
    public int hunger;

    public int energy;
    public int mana;

    public void SetUp(CharacterEntry characterData)
    {
        character = characterData;
        character.ApplyCharacter();
        character.Recalculate();

        health = character.maxHealth.CurrStat;
        hunger = character.hungerSize.CurrStat;

        energy = character.maxEnergy.CurrStat;
        mana = character.maxMana.CurrStat;
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
            _dgGameManager.OnCharacterDeath(this);
            if (GlobalGameManager.Instance.party.Contains(attacker.character))
            {
                _dungeonUI.AddEntry(character.ExperienceAward + " XP was awarded to the whole party!");
                List<CharacterEntry> levelledUp = new List<CharacterEntry>();
                List<int> changes = new List<int>();
                foreach (var member in GlobalGameManager.Instance.party)
                {
                    int added = member.GainXP(character.ExperienceAward);
                    if (added > 0)
                    {
                        levelledUp.Add(member);
                        changes.Add(added);
                    }
                }
                if (levelledUp.Count > 0)
                {
                    GlobalCanvasManager.Instance.LevelUpHandler.LevelUpSequence(levelledUp, changes);
                    if (levelledUp.Contains(GlobalGameManager.Instance.party[0]))
                    {
                        foreach (var entity in _dgGameManager.turnList)
                        {
                            if (entity is DGPlayer)
                            {
                                var plr = entity as DGPlayer;
                                plr.OnLeaderLevelChanged.Invoke();
                                break;
                            }
                        }
                    }
                }
            }
            _dgGameManager.RegisterRemoval(_entity);
        }
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
    }
    public void Consume(int amount, CHARACTER_STAT toConsume)
    {
        switch (toConsume)
        {
            case CHARACTER_STAT.ENERGY:
                energy = Mathf.Clamp(energy - amount, 0, character.maxEnergy.CurrStat);
                break;
            case CHARACTER_STAT.MANA:
                mana = Mathf.Clamp(mana - amount, 0, character.maxMana.CurrStat);
                break;
        }
    }

    public CharacterBehaviour HitDetect(TileCoord position)
    {
        TileInfo detectedTile = _entity.Floor.tiles[_entity.Floor.CoordToIndex(position)];
        DGEntity hit = detectedTile.occupyingEntity;
        if (hit != null)
        {
            var target = hit.GetComponent<CharacterBehaviour>();
            if (target.alliance != alliance)
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
    }

    public string GetDescription()
    {
        StringBuilder description = new StringBuilder();
        description.AppendLine("Level: " + character.characterLevel);
        description.AppendLine("EXP: " + character.experiencePoints + " / " + character.ExpToNextLevel);
        description.AppendLine("Held Item: " + character.HeldItem.ToString());
        description.Append("\n");
        description.AppendLine("HP: " + health + " / " + character.maxHealth.CurrStat + " | Hunger: " + hunger + " / " + character.hungerSize.CurrStat);
        description.AppendLine("Energy: " + energy + " / " + character.maxEnergy.CurrStat + " | Mana: " + mana + " / " + character.maxMana.CurrStat);
        description.Append("\n");
        description.AppendLine("PA: " + character.physAtk.CurrStat + " | PD: " + character.physDef.CurrStat);
        description.AppendLine("MA: " + character.magicAtk.CurrStat + " | MD: " + character.magicDef.CurrStat);

        return description.ToString();
    }

    public bool PerformMove(CombatMove move)
    {
        if (!move.CanBePerformed(this)) return false;
        move.Perform(this);
        return true;
    }
    private void Start()
    {
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _entity = GetComponent<DGEntity>();
        _dgGameManager = FindAnyObjectByType<DGGameManager>();
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
        _dungeonUI.UpdateLeaderInfo();
    }
}
