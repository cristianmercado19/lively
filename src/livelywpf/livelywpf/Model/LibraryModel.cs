﻿using livelywpf.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage.Provider;

namespace livelywpf
{
    public enum LibraryTileType
    {
        [Description("Converting to mp4")]
        videoConvert,
        [Description("To be added to library")]
        processing,
        installing,
        downloading,
        [Description("Ready to be used")]
        ready
    }

    [Serializable]
    public class LibraryModel : ObservableObject
    {
        public LibraryModel(LivelyInfoModel data, string folderPath, LibraryTileType tileType = LibraryTileType.ready)
        {
            DataType = tileType;
            LivelyInfo = new LivelyInfoModel(data);
            Title = data.Title;
            Desc = data.Desc;
            Author = data.Author;
            WallpaperType = FileFilter.GetLocalisedWallpaperTypeText(data.Type);
            SrcWebsite = GetUri(data.Contact, "https");

            if (data.IsAbsolutePath)
            {
                //full filepath is stored in Livelyinfo.json metadata file.
                FilePath = data.FileName;

                //This is to keep backward compatibility with older wallpaper files.
                //When I originally made the property all the paths where made absolute, not just wallpaper path.
                //But previewgif and thumb are always inside the temporary lively created folder.
                try
                {
                    //PreviewClipPath = data.Preview;
                    PreviewClipPath = Path.Combine(folderPath, Path.GetFileName(data.Preview));
                }
                catch
                {
                    PreviewClipPath = null;
                }

                try
                {
                    //ThumbnailPath = data.Thumbnail;
                    ThumbnailPath = Path.Combine(folderPath, Path.GetFileName(data.Thumbnail));
                }
                catch
                {
                    ThumbnailPath = null;
                }

                try
                {
                    LivelyPropertyPath = Path.Combine(Directory.GetParent(data.FileName).ToString(), "LivelyProperties.json");
                }
                catch
                {
                    LivelyPropertyPath = null;
                }
            }
            else
            {
                //Only relative path is stored, this will be inside "Lively Wallpaper" folder.
                if (data.Type == livelywpf.WallpaperType.url
                || data.Type == livelywpf.WallpaperType.videostream)
                {
                    //no file.
                    FilePath = data.FileName;
                }
                else
                {
                    try
                    {
                        FilePath = Path.Combine(folderPath, data.FileName);
                    }
                    catch
                    {
                        FilePath = null;
                    }

                    try
                    {
                        LivelyPropertyPath = Path.Combine(folderPath, "LivelyProperties.json");
                    }
                    catch
                    {
                        LivelyPropertyPath = null;
                    }
                }

                try
                {
                    PreviewClipPath = Path.Combine(folderPath, data.Preview);
                }
                catch
                {
                    PreviewClipPath = null;
                }

                try
                {
                    ThumbnailPath = Path.Combine(folderPath, data.Thumbnail);
                }
                catch
                {
                    ThumbnailPath = null;
                }
            }

            LivelyInfoFolderPath = folderPath;
            if (Program.SettingsVM.Settings.LivelyGUIRendering == LivelyGUIState.normal)
            {
                //Use animated gif if exists.
                ImagePath = File.Exists(PreviewClipPath) ? PreviewClipPath : ThumbnailPath;
            }
            else if (Program.SettingsVM.Settings.LivelyGUIRendering == LivelyGUIState.lite)
            {
                ImagePath = ThumbnailPath;
            }

            if (data.Type == livelywpf.WallpaperType.video || data.Type == livelywpf.WallpaperType.videostream)
            {
                //No user made livelyproperties file if missing, using default for video.
                if (LivelyPropertyPath == null)
                {
                    LivelyPropertyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "plugins", "libMPVPlayer", "api", "LivelyProperties.json");
                }
            }

            ItemStartup = false;
        }

        private LivelyInfoModel _livelyInfo;
        public LivelyInfoModel LivelyInfo
        {
            get
            {
                return _livelyInfo;
            }
            set
            {
                _livelyInfo = value;
                OnPropertyChanged("LivelyInfo");
            }
        }

        private LibraryTileType _dataType;
        public LibraryTileType DataType
        {
            get { return _dataType; }
            set
            {
                _dataType = value;
                OnPropertyChanged("DataType");
            }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (LivelyInfo.Type == livelywpf.WallpaperType.url
                || LivelyInfo.Type == livelywpf.WallpaperType.videostream)
                {
                    _filePath = value;
                }
                else
                {
                    _filePath = File.Exists(value) ? value : null;
                }
                OnPropertyChanged("FilePath");
            }
        }

        private string _livelyInfoFolderPath;
        public string LivelyInfoFolderPath
        {
            get { return _livelyInfoFolderPath; }
            set
            {
                _livelyInfoFolderPath = value;
                OnPropertyChanged("LivelyInfoFolderPath");
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = string.IsNullOrWhiteSpace(value) ? "---" : value;
                OnPropertyChanged("Author");
            }
        }

        private string _desc;
        public string Desc
        {
            get
            {
                return _desc;
            }
            set
            {
                _desc = string.IsNullOrWhiteSpace(value) ? "---" : value;
                OnPropertyChanged("Desc");
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                _imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        private string _previewClipPath;
        public string PreviewClipPath
        {
            get
            {
                return _previewClipPath;
            }
            set
            {
                _previewClipPath = File.Exists(value) ? value : null;
                OnPropertyChanged("PreviewClipPath");
            }
        }

        private string _thumbnailPath;
        public string ThumbnailPath
        {
            get
            {
                return _thumbnailPath;
            }
            set
            {
                _thumbnailPath = File.Exists(value) ? value : null;
                OnPropertyChanged("ThumbnailPath");
            }
        }

        private Uri _srcWebsite;
        public Uri SrcWebsite
        {
            get
            {
                return _srcWebsite;
            }
            set
            {
                _srcWebsite = value;
                OnPropertyChanged("SrcWebsite");
            }
        }

        private string _wallpaperType;
        /// <summary>
        /// Localised wallpapertype text.
        /// </summary>
        public string WallpaperType
        {
            get
            {
                return _wallpaperType;
            }
            set
            {
                _wallpaperType = value;
                OnPropertyChanged("WallpaperType");
            }
        }

        private string _livelyPropertyPath;
        /// <summary>
        /// LivelyProperties.json filepath if present, null otherwise.
        /// </summary>
        public string LivelyPropertyPath
        {
            get { return _livelyPropertyPath; }
            set
            {
                _livelyPropertyPath = File.Exists(value) ? value : null;
                OnPropertyChanged("LivelyPropertyPath");
            }
        }

        private bool _itemStartup;
        public bool ItemStartup
        {
            get { return _itemStartup; }
            set
            {
                _itemStartup = value;
                OnPropertyChanged("ItemStartup");
            }
        }

        #region helpers

        public Uri GetUri(string s, string scheme)
        {
            try
            {
                return new UriBuilder(s)
                {
                    Scheme = scheme,
                    Port = -1,
                }.Uri;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (UriFormatException)
            {
                return null;
            }
        }

        #endregion helpers
    }
}