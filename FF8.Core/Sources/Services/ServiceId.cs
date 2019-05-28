using System;

namespace FF8.Core
{
    public interface IServices
    {
        T Service<T>(ServiceId<T> id);
    }

    public abstract class ServiceId<T>
    {
        public Boolean IsSupported => false;
        public T this[IServices services] => services.Service(this);
    }

    public static class ServiceId
    {
        public static ServiceId<IFieldService> Field { get; } = new FieldServiceId();
        public static ServiceId<IInteractionService> Interaction { get; } = new InteractionServiceId();
        public static ServiceId<IGlobalVariableService> Global { get; } = new GlobalVariableServiceId();
        public static ServiceId<IGameplayService> Gameplay { get; } = new GameplayServiceId();
        public static ServiceId<ISalaryService> Salary { get; } = new SalaryServiceId();
        public static ServiceId<IPartyService> Party { get; } = new PartyServiceId();
        public static ServiceId<IMovieService> Movie { get; } = new MovieServiceId();
        public static ServiceId<IMessageService> Message { get; } = new MessageServiceId();
        public static ServiceId<IMenuService> Menu { get; } = new MenuServiceId();
        public static ServiceId<IMusicService> Music { get; } = new MusicServiceId();
        public static ServiceId<ISoundService> Sound { get; } = new SoundServiceId();
        public static ServiceId<IRenderingService> Rendering { get; } = new RenderingServiceId();

        private sealed class FieldServiceId : ServiceId<IFieldService>, IFieldService
        {
            public EventEngine Engine => throw new NotSupportedException();
            public void FadeOn() => throw new NotSupportedException();
            public void FadeOff() => throw new NotSupportedException();
            public void FadeIn() => throw new NotSupportedException();
            public void FadeOut() => throw new NotSupportedException();
            public void PrepareGoTo(FieldId fieldId) => throw new NotSupportedException();
            public void GoTo(FieldId fieldId, Int32 walkmeshId) => throw new NotSupportedException();
            public void BindArea(Int32 areaId) => throw new NotSupportedException();
        }

        private sealed class InteractionServiceId : ServiceId<IInteractionService>, IInteractionService
        {
            public Int32 this[ScriptResultId id]
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public IAwaitable Wait(Int32 frameNumber) => throw new NotSupportedException();
        }

        private sealed class GlobalVariableServiceId : ServiceId<IGlobalVariableService>, IGlobalVariableService
        {
            public T Get<T>(GlobalVariableId<T> id) where T : unmanaged => throw new NotSupportedException();
            public void Set<T>(GlobalVariableId<T> id, T value) where T : unmanaged => throw new NotSupportedException();
        }

        private sealed class GameplayServiceId : ServiceId<IGameplayService>, IGameplayService
        {
            public Boolean IsUserControlEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public Boolean IsRandomBattlesEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public void ResetAllData() => throw new NotSupportedException();
        }

        private sealed class SalaryServiceId : ServiceId<ISalaryService>, ISalaryService
        {
            public Boolean IsSalaryEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public Boolean IsSalaryAlertEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
        }

        private sealed class PartyServiceId : ServiceId<IPartyService>, IPartyService
        {
            public Boolean IsPartySwitchEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public void AddPlayableCharacter(CharacterId characterId) => throw new NotSupportedException();
            public void RemovePlayableCharacter(CharacterId characterId) => throw new NotSupportedException();
            public void AddPartyCharacter(CharacterId characterId) => throw new NotSupportedException();
            public void RemovePartyCharacter(CharacterId characterId) => throw new NotSupportedException();
            public void ChangeCharacterState(CharacterId characterId, Boolean isSwitchable, Boolean isSelectable) => throw new NotSupportedException();
            public void ChangeParty(CharacterId characterId1, CharacterId characterId2, CharacterId characterId3) => throw new NotSupportedException();
            public FieldObject FindPartyCharacterObject(Int32 partyId) => throw new NotSupportedException();
        }

        private sealed class MovieServiceId : ServiceId<IMovieService>, IMovieService
        {
            public void PrepareToPlay(Int32 movieId, Boolean flag) => throw new NotSupportedException();
            public void Play() => throw new NotSupportedException();
            public void Wait() => throw new NotSupportedException();
        }

        private sealed class MessageServiceId : ServiceId<IMessageService>, IMessageService
        {
            public void Show(Int32 channel, Int32 messageId) => throw new NotSupportedException();
            public void Show(Int32 channel, Int32 messageId, Int32 posX, Int32 posY) => throw new NotSupportedException();
            public void Close(Int32 channel) => throw new NotSupportedException();
            public IAwaitable ShowDialog(Int32 channel, Int32 messageId, Int32 posX, Int32 posY) => throw new NotSupportedException();
            public IAwaitable ShowQuestion(Int32 channel, Int32 messageId, Int32 firstLine, Int32 lastLine, Int32 beginLine, Int32 cancelLine) => throw new NotSupportedException();
            public IAwaitable ShowQuestion(Int32 channel, Int32 messageId, Int32 firstLine, Int32 lastLine, Int32 beginLine, Int32 cancelLine, Int32 posX, Int32 posY) => throw new NotSupportedException();
        }

        private sealed class MenuServiceId : ServiceId<IMenuService>, IMenuService
        {
            public IAwaitable ShowEnterNameDialog(NamedEntity entity) => throw new NotSupportedException();
        }

        private sealed class MusicServiceId : ServiceId<IMusicService>, IMusicService
        {
            public void ChangeBattleMusic(MusicId musicId) => throw new NotSupportedException();
            public void LoadFieldMusic(MusicId musicId) => throw new NotSupportedException();
            public void PlayFieldMusic() => throw new NotSupportedException();
            public void ChangeMusicVolume(Int32 volume, Boolean flag) => throw new NotSupportedException();
            public void ChangeMusicVolume(Int32 volume, Boolean flag, Int32 transitionDuration) => throw new NotSupportedException();
        }

        private sealed class SoundServiceId : ServiceId<ISoundService>, ISoundService
        {
            public void PlaySound(Int32 fieldSoundIndex, Int32 pan, Int32 volume, Int32 channel) => throw new NotSupportedException();
        }

        private sealed class RenderingServiceId : ServiceId<IRenderingService>, IRenderingService
        {
            public Int32 BackgroundFPS
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public void AddScreenColor(RGBColor rgbColor) => throw new NotSupportedException();
            public void SubScreenColor(RGBColor rgbColor) => throw new NotSupportedException();
            public void AddScreenColorTransition(RGBColor rgbColor, RGBColor offset, Int32 transitionDuration) => throw new NotSupportedException();
            public void SubScreenColorTransition(RGBColor rgbColor, RGBColor offset, Int32 transitionDuration) => throw new NotSupportedException();
            public IAwaitable Wait() => throw new NotSupportedException();
            public void AnimateBackground(Int32 firstFrame, Int32 lastFrame) => throw new NotSupportedException();
            public void DrawBackground() => throw new NotSupportedException();
        }
    }
}