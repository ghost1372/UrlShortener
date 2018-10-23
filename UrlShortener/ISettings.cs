using nucs.JsonSettings;

namespace UrlShortener
{
    public class ISettings : JsonSettings
    {
        public override string FileName { get; set; } = "TheDefaultFilename.extension"; //for loading and saving.

        #region Property

        public virtual bool TopMust { get; set; } = true;
        public virtual bool DefaultBitlyAPI { get; set; } = true;
        public virtual int? DefaultService { get; set; } = 0;
        public virtual string BitlyApiKey { get; set; } = "R_c597e397b606436fa6a9179626da61bb";
        public virtual string BitlyLoginKey { get; set; } = "o_1i6m8a9v55";
        public virtual string OpizoApiKey { get; set; } = "3DD3A7CD39B37BC8CBD9EFEEAC0B03DA";

        #endregion Property

        public ISettings()
        {
        }

        public ISettings(string fileName) : base(fileName)
        {
        }
    }
}