﻿namespace RoliSoft.TVShowTracker.FileNames
{
    using RoliSoft.TVShowTracker.ShowNames;

    /// <summary>
    /// Represents a TV show video file.
    /// </summary>
    public class ShowFile
    {
        /// <summary>
        /// Gets or sets the original name of the file.
        /// </summary>
        /// <value>The file name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the show.
        /// </summary>
        /// <value>The show.</value>
        public string Show { get; set; }

        /// <summary>
        /// Gets or sets the season of the episode.
        /// </summary>
        /// <value>The season.</value>
        public int Season { get; set; }

        /// <summary>
        /// Gets or sets the episode number.
        /// </summary>
        /// <value>The episode.</value>
        public int Episode { get; set; }

        /// <summary>
        /// Gets or sets the title of the episode.
        /// </summary>
        /// <value>The episode title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowFile"/> class.
        /// </summary>
        public ShowFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowFile"/> class.
        /// </summary>
        /// <param name="name">The name of the original file.</param>
        /// <param name="show">The name of the show.</param>
        /// <param name="ep">The parsed season and episode.</param>
        /// <param name="title">The title of the episode.</param>
        public ShowFile(string name, string show, ShowEpisode ep, string title)
        {
            Name    = name;
            Show    = show;
            Season  = ep.Season;
            Episode = ep.Episode;
            Title   = title;
        }
    }
}