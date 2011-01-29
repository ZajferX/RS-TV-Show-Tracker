﻿namespace RoliSoft.TVShowTracker.Parsers.Guides.Engines
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using HtmlAgilityPack;

    /// <summary>
    /// Provides support for scraping TV.com pages.
    /// </summary>
    public class TVcom : Guide
    {
        /// <summary>
        /// Extracts the data available in the database.
        /// </summary>
        /// <param name="id">The ID of the show.</param>
        /// <returns>TV show data.</returns>
        public override TVShow GetData(string id)
        {
            var summary = Utils.GetHTML("http://www.tv.com/{1}/show/{0}/summary.html".FormatWith(id.Split('\0')));
            var show    = new TVShow
                {
                    Title       = HtmlEntity.DeEntitize(summary.DocumentNode.GetNodeAttributeValue("//meta[@property='og:title']", "content")),
                    Genre       = Regex.Replace(summary.DocumentNode.GetTextValue("//h4[text()='Genre']/following-sibling::p[1]") ?? string.Empty, @"\s+", string.Empty).Replace(",", ", "),
                    Description = HtmlEntity.DeEntitize((summary.DocumentNode.GetTextValue("//p[starts-with(@class, 'show_description')]") ?? string.Empty).Replace("&hellip; More", string.Empty)),
                    Cover       = summary.DocumentNode.GetNodeAttributeValue("//meta[@property='og:image']", "content"),
                    Airing      = !Regex.IsMatch(summary.DocumentNode.GetTextValue("//h4[text()='Status']/following-sibling::p[1]") ?? string.Empty, "(Canceled|Ended)"),
                    Runtime     = 30,
                    Episodes    = new List<TVShow.Episode>()
                };

            var airinfo = summary.DocumentNode.GetTextValue("//span[@class='tagline']");
            if (airinfo != null)
            {
                airinfo = Regex.Replace(airinfo, @"\s+", " ").Trim();

                var airday = Regex.Match(airinfo, @"(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)");
                if (airday.Success)
                {
                    show.AirDay = airday.Groups[1].Value;
                }

                var airtime = Regex.Match(airinfo, @"(\d{1,2}:\d{2}(?: (?:AM|PM))?)", RegexOptions.IgnoreCase);
                if (airtime.Success)
                {
                    show.AirTime = airtime.Groups[1].Value;
                }

                var network = Regex.Match(airinfo, @"on ([^\s$\(]+)");
                if (network.Success)
                {
                    show.Network = network.Groups[1].Value;
                }
            }

            var listing = Utils.GetHTML("http://www.tv.com/{1}/show/{0}/episode.html?tag=list_header;paginator;All&season=All".FormatWith(id.Split('\0')));
            var episodes = listing.DocumentNode.SelectNodes("//li[starts-with(@class, 'episode')]");

            if (episodes == null)
            {
                return show;
            }

            DateTime dt;
            foreach (var episode in episodes.Reverse())
            {
                var meta   = episode.GetTextValue(".//div[@class='meta']");
                var season = Regex.Match(meta, "Season ([0-9]+)");
                var epnr   = Regex.Match(meta, "Episode ([0-9]+)");
                var aired  = Regex.Match(meta, @"Aired: (\d{1,2}/\d{1,2}/\d{4})");

                if (!season.Success || !epnr.Success)
                {
                    continue;
                }

                show.Episodes.Add(new TVShow.Episode
                    {
                        Season  = season.Groups[1].Value.ToInteger(),
                        Number  = epnr.Groups[1].Value.ToInteger(),
                        Airdate = DateTime.TryParseExact(aired.Groups[1].Value, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                                  ? dt
                                  : Utils.UnixEpoch,
                        Title   = HtmlEntity.DeEntitize(episode.GetTextValue(".//h3").Trim()),
                        Summary = HtmlEntity.DeEntitize(episode.GetTextValue(".//p[@class='synopsis']").Trim()),
                        Picture = episode.GetNodeAttributeValue(".//div[@class='THUMBNAIL']/a/img", "src")
                    });
            }

            return show;
        }

        /// <summary>
        /// Gets the ID of a TV show in the database.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>ID.</returns>
        public override string GetID(string name)
        {
            var html = Utils.GetHTML("http://www.tv.com/search.php?type=Search&stype=ajax_search&search_type=program&pg_results=0&sort=&qs=" + Uri.EscapeUriString(name));
            var url  = html.DocumentNode.GetNodeAttributeValue("//li//h2/a", "href");
            var id   = Regex.Match(url, @"[a-z]/([0-9]+)/[a-z]").Groups[1].Value + '\0' + Regex.Match(url, @"tv\.com/([^/]*)/").Groups[1].Value;
            return id;
        }
    }
}
