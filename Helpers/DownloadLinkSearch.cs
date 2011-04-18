﻿namespace RoliSoft.TVShowTracker.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using RoliSoft.TVShowTracker.Parsers.Downloads;

    /// <summary>
    /// Provides methods for searching download links on multiple provides asynchronously.
    /// </summary>
    public class DownloadSearch
    {
        /// <summary>
        /// Occurs when a download link search is done on all engines.
        /// </summary>
        public event EventHandler<EventArgs> DownloadSearchDone;

        /// <summary>
        /// Occurs when a download link search progress has changed.
        /// </summary>
        public event EventHandler<EventArgs<List<Link>, double, List<string>>> DownloadSearchProgressChanged;

        /// <summary>
        /// Occurs when a download link search has encountered an error.
        /// </summary>
        public event EventHandler<EventArgs<string, Exception>> DownloadSearchError;

        /// <summary>
        /// Gets or sets the search engines.
        /// </summary>
        /// <value>The search engines.</value>
        public List<DownloadSearchEngine> SearchEngines { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to filter search results.
        /// </summary>
        /// <value><c>true</c> if filtering is enabled; otherwise, <c>false</c>.</value>
        public bool Filter { get; set; }

        private volatile List<string> _remaining;
        private string[] _titleParts;
        private Regex _episodeRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubtitleSearch"/> class.
        /// </summary>
        /// <param name="engines">The engines to use for searching.</param>
        public DownloadSearch(IEnumerable<Type> engines = null)
        {
            if (engines == null)
            {
                engines = typeof(DownloadSearchEngine).GetDerivedTypes();
            }

            var remove = new List<DownloadSearchEngine>();

            SearchEngines = engines.Select(type => Activator.CreateInstance(type) as DownloadSearchEngine).ToList();

            foreach (var engine in SearchEngines)
            {
                engine.DownloadSearchDone  += SingleDownloadSearchDone;
                engine.DownloadSearchError += SingleDownloadSearchError;

                if (engine.Private)
                {
                    engine.Cookies = Settings.Get(engine.Name + " Cookies");

                    // if requires cookies and no cookies were provided, ignore the engine
                    if (string.IsNullOrWhiteSpace(engine.Cookies))
                    {
                        remove.Add(engine);
                    }
                }
            }

            // now remove them. if we remove it directly in the previous loop, an exception will be thrown that the enumeration was modified
            foreach (var engine in remove)
            {
                SearchEngines.Remove(engine);
            }
        }

        /// <summary>
        /// Searches for download links on multiple services asynchronously.
        /// </summary>
        /// <param name="query">The name of the release to search for.</param>
        public void SearchAsync(string query)
        {
            if (Filter)
            {
                if (ShowNames.Regexes.Numbering.IsMatch(query))
                {
                    var tmp       = ShowNames.Parser.Split(query);
                    _titleParts   = ShowNames.Parser.GetRoot(tmp[0]);
                    _episodeRegex = ShowNames.Parser.GenerateEpisodeRegexes(tmp[1]);
                }
                else
                {
                    _titleParts   = ShowNames.Parser.GetRoot(query);
                    _episodeRegex = null;
                }
            }

            _remaining = SearchEngines.Select(engine => engine.Name).ToList();
            query      = ShowNames.Parser.Normalize(query);

            SearchEngines.ForEach(engine => engine.SearchAsync(query));
        }

        /// <summary>
        /// Cancels the active asynchronous searches on all services.
        /// </summary>
        public void CancelAsync()
        {
            new Task(() => SearchEngines.ForEach(engine => engine.CancelAsync())).Start();
        }

        /// <summary>
        /// Called when a download link search is done.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SingleDownloadSearchDone(object sender, EventArgs<List<Link>> e)
        {
            try { _remaining.Remove((sender as DownloadSearchEngine).Name); } catch { }

            var percentage = (double)(SearchEngines.Count - _remaining.Count) / SearchEngines.Count * 100;

            if (Filter)
            {
                e.Data = e.Data
                          .Where(link => ShowNames.Parser.IsMatch(link.Release, _titleParts, _episodeRegex, false))
                          .ToList();
            }

            DownloadSearchProgressChanged.Fire(this, e.Data, percentage, _remaining);

            if (_remaining.Count == 0)
            {
                DownloadSearchDone.Fire(this);
            }
        }

        /// <summary>
        /// Called when a download link search has encountered an error.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SingleDownloadSearchError(object sender, EventArgs<string, Exception> e)
        {
            try { _remaining.Remove((sender as DownloadSearchEngine).Name); } catch { }

            DownloadSearchError.Fire(this, e.First, e.Second);

            if (_remaining.Count == 0)
            {
                DownloadSearchDone.Fire(this);
            }
        }
    }
}