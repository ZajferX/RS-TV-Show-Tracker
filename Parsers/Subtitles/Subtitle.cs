﻿namespace RoliSoft.TVShowTracker.Parsers.Subtitles
{
    /// <summary>
    /// Represents a subtitle.
    /// </summary>
    public class Subtitle
    {
        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>The site.</value>
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the release name.
        /// </summary>
        /// <value>The release name.</value>
        public string Release { get; set; }

        /// <summary>
        /// Gets or sets the language of the subtitle.
        /// </summary>
        /// <value>The language.</value>
        public Languages Language { get; set; }

        /// <summary>
        /// Gets or sets the URL to the subtitle.
        /// </summary>
        /// <value>The URL.</value>
        public string URL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the URL is a direct link to the download.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the URL is a direct link; otherwise, <c>false</c>.
        /// </value>
        public bool IsLinkDirect { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subtitle"/> class.
        /// </summary>
        public Subtitle()
        {
            IsLinkDirect = true;
        }
    }

    /// <summary>
    /// Represents the languages the search engines were designed to support.
    /// </summary>
    public enum Languages
    {
        /// <summary>
        /// Not specified or not recognized
        /// </summary>
        Unknown,
        /// <summary>
        /// English
        /// </summary>
        English,
        /// <summary>
        /// Hungarian - magyar
        /// </summary>
        Hungarian,
        /// <summary>
        /// Romanian - română
        /// </summary>
        Romanian
    }
}
