using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopStorageHandler : LayeredUI
{
    [SerializeField] CanvasGroup _group;
    [SerializeField] TMP_Text _title;
    [SerializeField] Transform _container;
    [SerializeField] Image _close;
    [SerializeField] TMP_Text _page;
    [SerializeField] GameObject _left, _right;
    [SerializeField] GameObject _listItem;
    [SerializeField] TMP_Text _infoText;
    [SerializeField] CanvasGroup _dialogueGrp;
    [SerializeField] Transform _dialogueContent;
    [SerializeField] CanvasGroup _readGrp;
    [SerializeField] Image _readCloseSelect;
    [SerializeField] TMP_Text _readTitle, _readContent;

    private FreeRoamMenuHandler _menu;
    private bool _isArmoury;
    List<Item> _shopList;
    private PromptInfo _merchantPrompt;
    private PromptInfo _warehousePrompt;
    public bool IsOpen
    {
        get { return _group.alpha > 0; }
    }

    private void CreateReadLayer(string title, string content)
    {
        var rLayer = new ReadLayer(_readGrp, _readCloseSelect);
        rLayer.functions = new List<MenuLayer.MenuFunction>();
        rLayer.refresh = delegate
        {
            _readTitle.text = title;
            _readContent.text = content;
        };
        rLayer.functions.Add(rLayer.Close);
        _layers.Add(rLayer);
        CurrentLayer.Open();
    }

    public void OpenShop(bool isArmoury)
    {
        _isArmoury = isArmoury;
        _shopList = _isArmoury ? GlobalGameManager.Instance.armouryShop : GlobalGameManager.Instance.merchantShop;
        var shopLayer = new ListLayer(_group, _page, _close, _container, _left, _right, _listItem);
        shopLayer.selectionChanged = delegate
        {
            if (shopLayer.CurrentSelection < shopLayer.functions.Count - 1)
            {
                var item = _shopList[shopLayer.CurrentSelection];
                _infoText.text = string.Format("Inventory\n{0} / {1}\n<color=#f6ff80>Price\n{2}\nYour Gold\n{3}</color>", 
                    GlobalGameManager.Instance.inventory.Count, GlobalGameManager.inventoryLimit, item.module.ShopPrice, GlobalGameManager.Instance.ownedGold);
            } else
            {
                _infoText.text = string.Empty;
            }
        };
        shopLayer.refresh = delegate
        {
            _title.text = _isArmoury ? "Armourer's Shop" : "Merchant's Shop";
            shopLayer.pageLimit = (int)Mathf.Ceil((float)_shopList.Count / ListLayer.pageMax);
            shopLayer.ClearList();
            shopLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = shopLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(shopLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0, _shopList.Count); i++)
            {
                var item = _shopList[i];
                var newEntry = shopLayer.AddEntry();
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = _shopList[i].ToString();
                if (GlobalGameManager.Instance.ownedGold < item.module.ShopPrice)
                {
                    newEntry.Find("ItemText").GetComponent<TMP_Text>().color = Color.red;
                }
                shopLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listItem);
                    dialogueLayer.refresh = delegate
                    {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();
                        var buyEntry = dialogueLayer.AddEntry();
                        buyEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Buy";
                        if (GlobalGameManager.Instance.ownedGold < item.module.ShopPrice)
                        {
                            buyEntry.Find("ItemText").GetComponent<TMP_Text>().color = Color.red;
                        }
                        dialogueLayer.functions.Add(delegate
                        {
                            bool purchase = GlobalGameManager.Instance.PurchaseItem(item);
                            if (purchase)
                            {
                                _shopList.Remove(item);
                                dialogueLayer.Close();
                            }
                        });

                        var infoEntry = dialogueLayer.AddEntry();
                        infoEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate
                        {
                            CreateReadLayer("About: " + item.ToString(), item.module.itemDescription);
                        });


                        var closeEntry = dialogueLayer.AddEntry();
                        closeEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Back";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    CurrentLayer.Open();
                });
            };
            shopLayer.functions.Add(shopLayer.Close);
        };
        _layers.Add(shopLayer);
        CurrentLayer.Open();
    }

    public void OpenSell()
    {
        var sellLayer = new ListLayer(_group, _page, _close, _container, _left, _right, _listItem);
        sellLayer.selectionChanged = delegate
        {
            if (sellLayer.CurrentSelection < sellLayer.functions.Count - 1)
            {
                var item = GlobalGameManager.Instance.inventory[sellLayer.CurrentSelection];
                _infoText.text = string.Format("<color=#f6ff80>Your Gold\n{0}\nSell Value\n{1}</color>", GlobalGameManager.Instance.ownedGold, item.module.SellValue);
            }
            else
            {
                _infoText.text = string.Empty;
            }
        };
        sellLayer.refresh = delegate
        {
            _title.text = "Selling Items";
            sellLayer.pageLimit = (int)Mathf.Ceil((float)GlobalGameManager.Instance.inventory.Count / ListLayer.pageMax);
            sellLayer.ClearList();
            sellLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = sellLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(sellLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0, GlobalGameManager.Instance.inventory.Count); i++)
            {
                var item = GlobalGameManager.Instance.inventory[i];
                var newEntry = sellLayer.AddEntry();
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = GlobalGameManager.Instance.inventory[i].ToString();
                sellLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listItem);
                    dialogueLayer.refresh = delegate
                    {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();
                        var sellEntry = dialogueLayer.AddEntry();
                        sellEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Sell";
                        dialogueLayer.functions.Add(delegate
                        {
                            GlobalGameManager.Instance.SellItem(sellLayer.CurrentSelection);
                            dialogueLayer.Close();
                        });

                        var infoEntry = dialogueLayer.AddEntry();
                        infoEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate
                        {
                            CreateReadLayer("About: " + item.ToString(), item.module.itemDescription);
                        });


                        var closeEntry = dialogueLayer.AddEntry();
                        closeEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Back";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    CurrentLayer.Open();
                });
            };
            sellLayer.functions.Add(sellLayer.Close);
        };
        _layers.Add(sellLayer);
        CurrentLayer.Open();
    }
    public void OpenStorage()
    {
        var storageLayer = new ListLayer(_group, _page, _close, _container, _left, _right, _listItem);
        storageLayer.refresh = delegate
        {
            _title.text = "Storage";
            _infoText.text = string.Format("Inventory\n{0} / {1}\nStorage\n{2} / {3}",
                GlobalGameManager.Instance.inventory.Count, GlobalGameManager.inventoryLimit,
                GlobalGameManager.Instance.storage.Count, GlobalGameManager.Instance.storageLimit);

            storageLayer.pageLimit = (int)Mathf.Ceil((float)GlobalGameManager.Instance.storage.Count / ListLayer.pageMax);
            storageLayer.ClearList();
            storageLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = storageLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(storageLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0, GlobalGameManager.Instance.storage.Count); i++)
            {
                var item = GlobalGameManager.Instance.storage[i];
                var newEntry = storageLayer.AddEntry();
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = GlobalGameManager.Instance.storage[i].ToString();
                storageLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listItem);
                    dialogueLayer.refresh = delegate
                    {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();
                        var sellEntry = dialogueLayer.AddEntry();
                        sellEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Retrieve";
                        dialogueLayer.functions.Add(delegate
                        {
                            GlobalGameManager.Instance.RetrieveItem(storageLayer.CurrentSelection);
                            dialogueLayer.Close();
                        });

                        var infoEntry = dialogueLayer.AddEntry();
                        infoEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate
                        {
                            CreateReadLayer("About: " + item.ToString(), item.module.itemDescription);
                        });


                        var closeEntry = dialogueLayer.AddEntry();
                        closeEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Back";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    CurrentLayer.Open();
                });
            };
            storageLayer.functions.Add(storageLayer.Close);
        };
        _layers.Add(storageLayer);
        CurrentLayer.Open();
    }

    public void OpenInventory()
    {
        var inventoryLayer = new ListLayer(_group, _page, _close, _container, _left, _right, _listItem);
        inventoryLayer.refresh = delegate
        {
            _title.text = "Inventory";
            _infoText.text = string.Format("Inventory\n{0} / {1}\nStorage\n{2} / {3}", 
                GlobalGameManager.Instance.inventory.Count, GlobalGameManager.inventoryLimit,
                GlobalGameManager.Instance.storage.Count, GlobalGameManager.Instance.storageLimit);

            inventoryLayer.pageLimit = (int)Mathf.Ceil((float)GlobalGameManager.Instance.inventory.Count / ListLayer.pageMax);
            inventoryLayer.ClearList();
            inventoryLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = inventoryLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(inventoryLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0, GlobalGameManager.Instance.inventory.Count); i++)
            {
                var item = GlobalGameManager.Instance.inventory[i];
                var newEntry = inventoryLayer.AddEntry();
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = GlobalGameManager.Instance.inventory[i].ToString();
                inventoryLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listItem);
                    dialogueLayer.refresh = delegate
                    {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();
                        var sellEntry = dialogueLayer.AddEntry();
                        sellEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Store";
                        dialogueLayer.functions.Add(delegate
                        {
                            GlobalGameManager.Instance.StoreItem(inventoryLayer.CurrentSelection);
                            dialogueLayer.Close();
                        });

                        var infoEntry = dialogueLayer.AddEntry();
                        infoEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate
                        {
                            CreateReadLayer("About: " + item.ToString(), item.module.itemDescription);
                        });


                        var closeEntry = dialogueLayer.AddEntry();
                        closeEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Back";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    CurrentLayer.Open();
                });
            };
            inventoryLayer.functions.Add(inventoryLayer.Close);
        };
        _layers.Add(inventoryLayer);
        CurrentLayer.Open();
    }
    public void OpenMerchantPrompt()
    {
        GlobalCanvasManager.Instance.PromptHandler.Prompt(_merchantPrompt);
    }

    public void OpenWarehousePrompt()
    {
        GlobalCanvasManager.Instance.PromptHandler.Prompt(_warehousePrompt);
    }

    private new void Start()
    {
        base.Start();
        _inputManager = FindAnyObjectByType<PlayerInput>();
        _menu = GlobalCanvasManager.Instance.FreeRoamMenuHandler;

        _merchantPrompt = PromptInfo.New("What would you like to do?", new string[] { "Leave", "Buy Items", "Sell Items" }, new PromptInfo.OptionFunction[]
        {
            PromptInfo.NullFunction,
            delegate
            {
                OpenShop(false);
            },
            delegate
            {
                OpenSell();
            }
        });

        _warehousePrompt = PromptInfo.New("What would you like to do?", new string[] { "Leave", "Store Items", "Retrieve Items" }, new PromptInfo.OptionFunction[]
        {
            PromptInfo.NullFunction,
            delegate
            {
                OpenInventory();
            },
            delegate
            {
                OpenStorage();
            }
        });
    }

    private void Update()
    {
        Process();
        _menu.enabled = !IsOpen;
    }
}
