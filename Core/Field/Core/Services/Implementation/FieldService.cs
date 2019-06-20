using System;

namespace OpenVIII
{
    public sealed class FieldService : IFieldService
    {
        public Boolean IsSupported => true;
        public EventEngine Engine { get; }

        public FieldService(EventEngine engine)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public void FadeOn()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeOn)}()");
        }

        public void FadeOff()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeOff)}()");
        }

        public void FadeIn()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeIn)}()");
        }

        public void FadeOut()
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(FadeOut)}()");
        }

        public void PrepareGoTo(int fieldId)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(PrepareGoTo)}({FieldId.FieldId_[fieldId]}: {fieldId})");
        }

        public void GoTo(int fieldId, Int32 walkmeshId)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(GoTo)}({FieldId.FieldId_[fieldId]}: {fieldId}, {nameof(walkmeshId)}: {walkmeshId})");
        }

        public void BindArea(Int32 areaId)
        {
            // TODO: Field script
            Console.WriteLine($"NotImplemented: {nameof(FieldService)}.{nameof(BindArea)}({nameof(areaId)}: {areaId})");
        }
    }
}