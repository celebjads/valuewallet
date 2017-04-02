﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Info.Blockchain.API.BlockExplorer;
using Info.Blockchain.API.Client;
using Info.Blockchain.API.Data;
using Info.Blockchain.API.Json;
using Newtonsoft.Json;

namespace Info.Blockchain.API.Wallet
{
    /// <summary>
    /// This class reflects the functionality documented
    /// at https://blockchain.info/api/blockchain_wallet_api. It allows users to interact
    /// with their Blockchain.info wallet.
    /// </summary>
    public class Wallet
    {
        private readonly IHttpClient httpClient;
        private readonly string identifier;
        private readonly string password;
        private readonly string secondPassword;

        /// <summary>
        /// </summary>
        /// <param name="httpClient">IHttpClient to access Blockchain REST Api</param>
        /// <param name="identifier">Wallet identifier (GUID)</param>
        /// <param name="password">Decryption password</param>
        /// <param name="secondPassword">Second password</param>
        internal Wallet(IHttpClient httpClient, string identifier, string password, string secondPassword = null)
        {
            this.httpClient = httpClient;
            this.identifier = identifier;
            this.password = password;
            this.secondPassword = secondPassword;
        }

        /// <summary>
        /// Sends bitcoin from your wallet to a single address.
        /// </summary>
        /// <param name="toAddress">Recipient bitcoin address</param>
        /// <param name="amount">Amount to send</param>
        /// <param name="fromAddress">Specific address to send from</param>
        /// <param name="fee">Transaction fee. Must be greater than the default fee</param>
        /// <param name="note">Public note to include with the transaction</param>
        /// <returns>An instance of the PaymentResponse class</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<PaymentResponse> SendAsync(string toAddress, BitcoinValue amount,
            string fromAddress = null, BitcoinValue fee = null, string note = null)
        {
            if (string.IsNullOrWhiteSpace(toAddress))
            {
                throw new ArgumentNullException(nameof(toAddress));
            }
            if (amount.GetBtc() <= 0)
            {
                throw new ArgumentException("Amount sent must be greater than 0", nameof(amount));
            }

            QueryString queryString = new QueryString();
            queryString.Add("password", password);
            queryString.Add("to", toAddress);
            queryString.Add("amount", amount.Satoshis.ToString());
            if (!string.IsNullOrWhiteSpace(secondPassword))
            {
                queryString.Add("second_password", secondPassword);
            }
            if (!string.IsNullOrWhiteSpace(fromAddress))
            {
                queryString.Add("from", fromAddress);
            }
            if (!string.IsNullOrWhiteSpace(note))
            {
                queryString.Add("note", note);
            }
            if (fee != null)
            {
                queryString.Add("fee", fee.ToString());
            }

            string route = $"merchant/{identifier}/payment";

            PaymentResponse paymentResponse = await httpClient.GetAsync<PaymentResponse>(route, queryString);
            return paymentResponse;
        }

        /// <summary>
        /// Sends bitcoin from your wallet to multiple addresses.
        /// </summary>
        /// <param name="recipients">Dictionary with the structure of 'address':amount
        /// (string:BitcoinValue)</param>
        /// <param name="fromAddress">Specific address to send from</param>
        /// <param name="fee">Transaction fee. Must be greater than the default fee</param>
        /// <param name="note">Public note to include with the transaction</param>
        /// <returns>An instance of the PaymentResponse class</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<PaymentResponse> SendManyAsync(Dictionary<string, BitcoinValue> recipients,
            string fromAddress = null, BitcoinValue fee = null, string note = null)
        {
            if (recipients == null || recipients.Count == 0)
            {
                throw new ArgumentException("Sending bitcoin from your wallet requires at least one receipient.", nameof(recipients));
            }

            QueryString queryString = new QueryString();
            queryString.Add("password", password);
            string recipientsJson = JsonConvert.SerializeObject(recipients, Formatting.None, new BitcoinValueJsonConverter());
            queryString.Add("recipients", recipientsJson);
            if (!string.IsNullOrWhiteSpace(secondPassword))
            {
                queryString.Add("second_password", secondPassword);
            }
            if (!string.IsNullOrWhiteSpace(fromAddress))
            {
                queryString.Add("from", fromAddress);
            }
            if (!string.IsNullOrWhiteSpace(note))
            {
                queryString.Add("note", note);
            }
            if (fee != null)
            {
                queryString.Add("fee", fee.ToString());
            }

            string route = $"merchant/{identifier}/sendmany";

            PaymentResponse paymentResponse = await httpClient.GetAsync<PaymentResponse>(route, queryString);

            return paymentResponse;
        }

        /// <summary>
        /// Fetches the wallet balance. Includes unconfirmed transactions
        /// and possibly double spends.
        /// </summary>
        /// <returns>Wallet balance</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<BitcoinValue> GetBalanceAsync()
        {
            QueryString queryString = BuildBasicQueryString();
            string route = $"merchant/{identifier}/balance";
            BitcoinValue bitcoinValue = await httpClient.GetAsync<BitcoinValue>(route, queryString);
            return bitcoinValue;
        }

        /// <summary>
        /// Lists all active addresses in the wallet.
        /// </summary>
        /// <param name="confirmations">Minimum number of confirmations transactions
        /// must have before being included in the balance of addresses (can be 0)</param>
        /// <returns>A list of Address objects</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<List<WalletAddress>> ListAddressesAsync(int confirmations = 0)
        {
            if (confirmations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(confirmations), "Confirmations must be a positive number");
            }
            QueryString queryString = BuildBasicQueryString();
            queryString.Add("confirmations", confirmations.ToString());

            string route = $"merchant/{identifier}/list";

            List<WalletAddress> addressList = await httpClient.GetAsync<List<WalletAddress>>(route, queryString, WalletAddress.DeserializeMultiple);
            return addressList;
        }

        /// <summary>
        /// Retrieves an address from the wallet.
        /// </summary>
        /// <param name="address"> Address in the wallet to look up</param>
        /// <param name="confirmations">Minimum number of confirmations transactions
        /// must have before being included in the balance of addresses (can be 0)</param>
        /// <returns>An instance of the Address class</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<WalletAddress> GetAddressAsync(string address, int confirmations = 0)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentNullException(nameof(address));
            }
            if (confirmations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(confirmations), "Confirmations must be a positive number");
            }
            QueryString queryString = BuildBasicQueryString();
            queryString.Add("confirmations", confirmations.ToString());
            queryString.Add("address", address);

            string route = $"merchant/{identifier}/address_balance";
            WalletAddress addressObj = await httpClient.GetAsync<WalletAddress>(route, queryString);
            return addressObj;
        }

        /// <summary>
        /// Generates a new address and adds it to the wallet.
        /// </summary>
        /// <param name="label">Label to attach to this address</param>
        /// <returns>An instance of the Address class</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<WalletAddress> NewAddress(string label = null)
        {
            QueryString queryString = BuildBasicQueryString();
            if (label != null)
            {
                queryString.Add("label", label);
            }
            string route = $"merchant/{identifier}/new_address";
            WalletAddress addressObj = await httpClient.GetAsync<WalletAddress>(route, queryString);
            return addressObj;
        }

        /// <summary>
        /// Archives an address.
        /// </summary>
        /// <param name="address">Address to archive</param>
        /// <returns>String representation of the archived address</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<string> ArchiveAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentNullException(nameof(address));
            }
            QueryString queryString = BuildBasicQueryString();
            queryString.Add("address", address);

            string route = $"merchant/{identifier}/archive_address";
            string archiveAddress = await httpClient.GetAsync<string>(route, queryString, WalletAddress.DeserializeArchived);

            return archiveAddress;
        }

        /// <summary>
        /// Unarchives an address.
        /// </summary>
        /// <param name="address">Address to unarchive</param>
        /// <returns>String representation of the unarchived address</returns>
        /// <exception cref="ServerApiException">If the server returns an error</exception>
        public async Task<string> UnarchiveAddress(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }
            QueryString queryString = BuildBasicQueryString();
            queryString.Add("address", address);

            string route = $"merchant/{identifier}/unarchive_address";
            string activeAddress = await httpClient.GetAsync<string>(route, queryString, WalletAddress.DeserializeUnArchived);
            return activeAddress;
        }

        private QueryString BuildBasicQueryString()
        {
            QueryString queryString = new QueryString();

            queryString.Add("password", password);
            if (secondPassword != null)
            {
                queryString.Add("second_password", secondPassword);
            }

            return queryString;
        }
	}
}