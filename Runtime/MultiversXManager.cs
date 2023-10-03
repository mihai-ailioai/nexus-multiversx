using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Proyecto26;
using Newtonsoft.Json;

namespace Nexus.MultiversX
{
    public class MultiversXManager : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void OpenPopup(string url, string callback);

        private struct AccountResponse
        {
            public string address;
            public string balance;
            public int nonce;
            public long timestamp;
            public int shard;
            public int txCount;
            public int scrCount;
            public string developerReward;
        }

        private struct NFTResponse
        {
            public string identifier;
        }

        [System.Serializable]
        public class RoomNFT
        {
            public GameObject roomBtn;
            public string nftId;
        }

        private static string _API_URL = "https://api.multiversx.com/";
        private static string _MULTIVERS_X_WEBHOOK = "https://wallet.multiversx.com/hook/login";
        
        public static MultiversXManager Instance { get; private set; }
        [SerializeField] private MultiversXMenu _button;
        [SerializeField] private List<TMPro.TMP_Text> _texts;
        [SerializeField] private TMPro.TMP_Text _balanceText;
        [SerializeField] private List<RoomNFT> _roomNFTs;

        private string _address;
        public string Address => _address;

        public string _walletAddress;

        private static string API_URL(string endpoint)
        {
            return _API_URL + endpoint;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                _address = Instance._address;
                Destroy(Instance.gameObject);
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (string.IsNullOrEmpty(_address))
            {
                _button.gameObject.SetActive(true);
                foreach (var _text in _texts) _text.gameObject.SetActive(false);
                _balanceText.gameObject.SetActive(false);
            }
            else
            {
                _button.gameObject.SetActive(false);
                PopulateAccountInfo();
                UnlockOwnedRooms();
            }
        }

        void PopulateAccountInfo()
        {
            string address = _address.Substring(0, 5) + "...." + _address.Substring(_address.Length - 5, 5);
            foreach (var _text in _texts)
            {
                _text.gameObject.SetActive(true);
                _text.text = address;
            }

            _balanceText.gameObject.SetActive(true);
            _balanceText.text = "Balance: loading...";
            RestClient.Get(API_URL($"accounts/{_address}"), (e, res) =>
            {
                if (res.StatusCode == 200)
                {
                    AccountResponse data = JsonConvert.DeserializeObject<AccountResponse>(res.Text);
                    _balanceText.text = $"Balance: 0.{data.balance}";
                }
            });
        }

        void UnlockOwnedRooms()
        {
            RestClient.Get(API_URL($"accounts/{_address}/nfts/"), (e, res) =>
            {
                if (res.StatusCode == 200)
                {
                    var data = JsonConvert.DeserializeObject<List<NFTResponse>>(res.Text);
                    foreach (var item in _roomNFTs)
                    {
                        foreach (var nft in data)
                        {
                            if (item.nftId == nft.identifier)
                            {
                                item.roomBtn.SetActive(false);
                                break;
                            }
                        }
                    }
                }
            });
        }

        public void TriggerPopup()
        {
#if UNITY_EDITOR
            OnPopupNavigationComplete(_walletAddress);
#else
            OpenPopup(_MULTIVERS_X_WEBHOOK, "webhooks");
#endif
        }

        public void OnPopupNavigationComplete(string url)
        {
            Debug.Log("Popup navigation completed! " + url);
            _address = url;

            _button.gameObject.SetActive(false);
            PopulateAccountInfo();
            UnlockOwnedRooms();
        }

        public void OpenRoomURL(GameObject sender)
        {
            foreach (var item in _roomNFTs)
            {
                if (item.roomBtn == sender)
                {
                    Application.OpenURL($"https://www.frameit.gg/marketplace/nft/{item.nftId}");
                    return;
                }
            }
        }

        public void MintNFT(GameObject sender)
        {
            // TODO: Mint NFT based on the sender object
            Application.OpenURL($"https://www.frameit.gg/");
        }
    }
}