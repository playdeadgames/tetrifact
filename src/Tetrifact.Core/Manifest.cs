﻿using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Exposes the contents of a package. On filesystems, this is stored as manifest.json in the package root folder.
    /// </summary>
    public class Manifest : Package
    {
        #region PROPERTIES
    
        /// <summary>
        /// Files in this package. Each file can be downloaded using its public id, and should be saved at its path.
        /// </summary>
        public IList<ManifestItem> Files { get; private set; }

        /// <summary>
        /// Combined size (bytes) of files (not linked files) in package.
        /// </summary>
        public long SizeOnDisk { get; set; }

        /// <summary>
        /// Size of files and linked files in package.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Optional - string id of package this package depends on.
        /// </summary>
        public string DependsOn { get; set; }

        /// <summary>
        /// Path on disk for this manifest file. This path will change if the manifest is changed.
        /// </summary>
        public string PathOnDisk { get; set; }

        /// <summary>
        /// Size of chunks (in bytes) for this package. 
        /// </summary>
        public long FileChunkSize { get; set; }

        /// <summary>
        /// Retrieves percentage saved with diff patching.
        /// </summary>
        public int Compressed 
        {
            get 
            {
                if (this.SizeOnDisk == 0 || this.Size == 0)
                    return 0;

                decimal p = (decimal)this.SizeOnDisk / (decimal)this.Size;
                return (int)System.Math.Round((decimal)(p * 100), 0);
            }
        }

        #endregion

        #region CTORS

        public Manifest()
        {
            this.Files = new List<ManifestItem>();
        }

        #endregion
    }
}
