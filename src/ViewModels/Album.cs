﻿using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using Symphony.Scrobbler;
using System.Reactive;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Symphony.ViewModels
{
    public class Album : ViewModelBase, IComparable<Album>
    {
        private IBitmap _cover;

        public Album()
        {
            Tracks = new List<Track>();

            GetArtworkCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                MainWindowViewModel.Instance.SelectArtwork.IsVisible = true;

                await MainWindowViewModel.Instance.SelectArtwork.QueryAlbumCoverAsync(this);
                /*var scraper = new AlbumArtworkScraper();

                var data = await scraper.DownloadArtwork("uk", Artist, Title);

                */
            });
        }

        public async Task UpdateCoverArt(string url)
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            using (var client = new HttpClient(clientHandler))
            {
                var data = await client.GetByteArrayAsync(url);

                if (data != null)
                {
                    using (var ms = new MemoryStream(data))
                    {
                        Cover = new Bitmap(ms);
                    }

                    foreach (var track in Tracks)
                    {
                        using (var tagFile = TagLib.File.Create(track.Path))
                        {
                            tagFile.Tag.Pictures = new TagLib.Picture[]
                            {
                                new TagLib.Picture(new TagLib.ByteVector(data, data.Length))
                                {
                                     Type = TagLib.PictureType.FrontCover,
                                     MimeType = "image/jpeg"
                                }
                            };

                            tagFile.Save();
                        }
                    }
                }
            }
        }

        public string Title { get; set; }

        public string Artist { get; set; }

        public List<Track> Tracks { get; set; }

        public IBitmap Cover
        {
            get { return _cover; }
            set { this.RaiseAndSetIfChanged(ref _cover, value); }
        }

        public ReactiveCommand<Unit, Unit> GetArtworkCommand { get; }

        public int CompareTo([AllowNull] Album other)
        {
            return Title.CompareTo(other.Title);
        }
    }
}
