namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which can remove unused files from the repository folder.
    /// </summary>
    public interface IRepositoryCleaner
    {
        /// <summary>
        /// Cleans dead files out from repository folder. When a package is deleted, it's contents in the repositiry folder stay behind. 
        /// Cleaning them out must be run seperately.
        /// </summary>
        void Clean();
    }
}
