﻿using InvestorApi.Contracts;
using InvestorApi.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace InvestorApi.Asx
{
    /// <summary>
    /// Implements a share finder and information provider using the public ASX company list.
    /// </summary>
    /// <seealso cref="InvestorApi.Contracts.IShareDetailsProvider" />
    internal class AsxShareDetailProvider : IShareDetailsProvider
    {
        private const string Address = "http://www.asx.com.au/asx/research/ASXListedCompanies.csv";

        private static readonly object _syncLock = new object();
        private static IDictionary<string, ShareDetails> _shares = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsxShareDetailProvider"/> class.
        /// </summary>
        public AsxShareDetailProvider()
        {
            Load();
        }

        /// <summary>
        /// Returns detailed information for the share with the provided symbol.
        /// </summary>
        /// <param name="symbol">The share symbol to retrun the details for.</param>
        /// <returns>The share details.</returns>
        public ShareDetails GetShareDetails(string symbol)
        {
            Load();

            if (_shares.TryGetValue(symbol, out ShareDetails result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Returns detailed information for the shares with the provided symbols.
        /// </summary>
        /// <param name="symbols">The share symbols to retrun the details for.</param>
        /// <returns>The share details.</returns>
        public IReadOnlyDictionary<string, ShareDetails> GetShareDetails(IEnumerable<string> symbols)
        {
            Load();

            return symbols
                .Distinct()
                .Select(symbol => GetShareDetails(symbol))
                .ToDictionary(share => share.Symbol);
        }

        /// <summary>
        /// Finds shares by the supplied criteria.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="industry">The industry.</param>
        /// <param name="pageNumber">Gets the page number to return.</param>
        /// <param name="pageSize">Gets the page size to apply.</param>
        /// <returns>The list of shares which match the search criteria.</returns>
        public ListResult<ShareDetails> FindShareDetails(string searchTerm, string industry, int pageNumber, int pageSize)
        {
            Load();

            var allResults = _shares
                .Select(share => share.Value)
                .Where(share =>
                    searchTerm == null ||
                    share.Symbol.Equals(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    share.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) > -1)
                .Where(share =>
                    industry == null ||
                    share.Industry == industry)
                .ToList();

            return new ListResult<ShareDetails>(
                allResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                pageNumber,
                pageSize,
                allResults.Count);
        }

        /// <summary>
        /// Download the CSV, parse the data and keep it in memory.
        /// </summary>
        private void Load()
        {
            if (_shares != null)
            {
                return;
            }

            lock (_syncLock)
            {
                if (_shares != null)
                {
                    return;
                }

                using (var client = new HttpClient())
                {
                    // Download and parse the CSV.
                    // Note that we need to skip the first two lines because on contains headers and the second is blank.
                    var csv = client.GetStringAsync(Address).Result;
                    _shares = csv
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Skip(2)
                        .Select(line => ReadCsvLine(line))
                        .Where(line => line != null)
                        .ToDictionary(line => line.Symbol);
                }
            }
        }

        private ShareDetails ReadCsvLine(string line)
        {
            var values = line.Split(',');
            if (values.Length != 3)
            {
                return null;
            }

            var name = values[0].Substring(1, values[0].Length - 2);
            var symbol = values[1];
            var industry = values[2].Substring(1, values[2].Length - 2);

            if (industry == "Not Applic")
            {
                industry = null;
            }

            return new ShareDetails(symbol, name, industry);
        }
    }
}
