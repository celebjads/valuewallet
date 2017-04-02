using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Info.Blockchain.API.Client;
using Info.Blockchain.API.BlockExplorer;
using Info.Blockchain.API.Wallet;

namespace Info.Blockchain.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Address")]
    public class AddressController : Controller
    {
        private string _APiCode = "6f987933-55e8-4dba-9b14-4ba57e5e29ba";


        [HttpGet]
        [Route("Transactions")]
        public async Task<Transaction> GetTransaction()
        {
            try
            {

                BlockchainApiHelper BlockchainApiHelper = new BlockchainApiHelper(_APiCode, new BlockchainHttpClient(), _APiCode, new BlockchainHttpClient());
                Task<Transaction> returnedTaskTResult = BlockchainApiHelper.blockExplorer.GetTransactionByIndexAsync(0);
                Transaction intResult = await returnedTaskTResult;
                return intResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        
        [HttpGet]
        [Route("UnSpent")]
        public async Task<ReadOnlyCollection<UnspentOutput>> GetUnspentOutputs()
        {
            BlockchainApiHelper BlockchainApiHelper = new BlockchainApiHelper(_APiCode, new BlockchainHttpClient(), _APiCode, new BlockchainHttpClient());
            Task<ReadOnlyCollection<UnspentOutput>> returnedTaskTResult = BlockchainApiHelper.blockExplorer.GetUnspentOutputsAsync("1NS6qad5NchWAkwGvmgkLpKjbAyrpj6rZy");
            ReadOnlyCollection<UnspentOutput> intResult = await returnedTaskTResult;
            return intResult;
        }
        [HttpGet]
        [Route("Statics")]
        public ActionResult GetStatics()
        {
            BlockchainApiHelper BlockchainApiHelper = new BlockchainApiHelper(_APiCode, new BlockchainHttpClient(), _APiCode, new BlockchainHttpClient());
            Task<Address> returnedTaskTResult = GetStats();
            Address intResult = returnedTaskTResult.Result;
            ViewBag.Address = intResult.AddressStr;
            ViewBag.Hash160 = intResult.Hash160;
            ViewBag.Transactions = intResult.TransactionCount;
            ViewBag.Received = intResult.TotalReceived;
            ViewBag.Balance = intResult.FinalBalance;
            return View();
        }
        private async Task<Address>  GetStats()
        {
            BlockchainApiHelper BlockchainApiHelper = new BlockchainApiHelper(_APiCode, new BlockchainHttpClient(), _APiCode, new BlockchainHttpClient());
            Task<Address> returnedTaskTResult = BlockchainApiHelper.blockExplorer.GetAddressAsync("1NS6qad5NchWAkwGvmgkLpKjbAyrpj6rZy", 0);
            Address intResult = await returnedTaskTResult;
            return intResult;
        }
        [HttpGet]
        [Route("List")]
        public async Task<List<WalletAddress>> GetList()
        {
            try
            {

            
            BlockchainApiHelper BlockchainApiHelper = new BlockchainApiHelper(_APiCode, new BlockchainHttpClient(), _APiCode, new BlockchainHttpClient());
            Wallet.Wallet wallet = BlockchainApiHelper.CreateWallet("d3a3ab4c-dfef-4c6f-b784-6a5686abffea", "blockchain269");
            Task<List<WalletAddress>> returnedTaskTResult =  wallet.ListAddressesAsync(0);
            List<WalletAddress> intResult = await returnedTaskTResult;
            return intResult;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}