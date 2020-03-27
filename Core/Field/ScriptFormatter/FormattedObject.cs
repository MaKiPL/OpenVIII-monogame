namespace OpenVIII.Fields.Scripts
{
    public sealed class FormattedObject
    {
        #region Constructors

        public FormattedObject(Field.Info field, string objectName, string scripts)
        {
            Field = field;
            ObjectName = objectName;
            Scripts = scripts;
        }

        #endregion Constructors

        #region Properties

        public Field.Info Field { get; }
        public string ObjectName { get; }
        public string Scripts { get; }

        #endregion Properties
    }
}