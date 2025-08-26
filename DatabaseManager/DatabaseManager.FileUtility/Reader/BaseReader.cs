namespace DatabaseManager.FileUtility
{
    public abstract class BaseReader    {
      
        protected ImportDataInfo info;

        public BaseReader(ImportDataInfo info)
        {
            this.info = info;
        }

        public abstract DataReadResult Read();
    }
}
