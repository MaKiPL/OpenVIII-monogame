using System;

namespace OpenVIII.Fields
{
    public sealed class FieldService : IFieldService
    {
        #region Constructors

        public FieldService(EventEngine engine) => Engine = engine ?? throw new ArgumentNullException(nameof(engine));

        #endregion Constructors

        #region Properties

        public EventEngine Engine { get; }
        public bool IsSupported => true;

        #endregion Properties

        #region Methods

        public void BindArea(int areaId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(BindArea)}({nameof(areaId)}: {areaId})");

        public void FadeIn() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeIn)}()");

        public void FadeOff() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeOff)}()");

        public void FadeOn() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeOn)}()");

        public void FadeOut() =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeOut)}()");

        public void GoTo(int fieldId, int walkmeshId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(GoTo)}({FieldId.FieldId_[fieldId]}: {fieldId}, {nameof(walkmeshId)}: {walkmeshId})");

        public void PrepareGoTo(int fieldId) =>
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(PrepareGoTo)}({FieldId.FieldId_[fieldId]}: {fieldId})");

        #endregion Methods
    }
}