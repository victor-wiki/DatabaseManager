namespace DatabaseManager.FileUtility
{
    public abstract class BaseReader
    {

        protected SourceFileInfo info;

        public BaseReader(SourceFileInfo info)
        {
            this.info = info;
        }

        public abstract DataReadResult Read(bool onlyReadHeader = false);
    }
}
