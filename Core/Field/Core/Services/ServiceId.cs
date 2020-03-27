using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.Fields
{
    public interface IServices
    {
        #region Methods

        T Service<T>(ServiceId<T> id);

        #endregion Methods
    }

    public static class ServiceId
    {
        #region Properties

        public static ServiceId<IFieldService> Field { get; } = new FieldServiceId();
        public static ServiceId<IGameplayService> Gameplay { get; } = new GameplayServiceId();
        public static ServiceId<IGlobalVariableService> Global { get; } = new GlobalVariableServiceId();
        public static ServiceId<IInteractionService> Interaction { get; } = new InteractionServiceId();
        public static ServiceId<IMenuService> Menu { get; } = new MenuServiceId();
        public static ServiceId<IMessageService> Message { get; } = new MessageServiceId();
        public static ServiceId<IMovieService> Movie { get; } = new MovieServiceId();
        public static ServiceId<IMusicService> Music { get; } = new MusicServiceId();
        public static ServiceId<IPartyService> Party { get; } = new PartyServiceId();
        public static ServiceId<IRenderingService> Rendering { get; } = new RenderingServiceId();
        public static ServiceId<ISalaryService> Salary { get; } = new SalaryServiceId();
        public static ServiceId<ISoundService> Sound { get; } = new SoundServiceId();

        #endregion Properties

        #region Classes

        private sealed class FieldServiceId : ServiceId<IFieldService>, IFieldService
        {
            #region Properties

            public EventEngine Engine => throw new NotSupportedException();

            #endregion Properties

            #region Methods

            public void BindArea(int areaId) => throw new NotSupportedException();

            public void FadeIn() => throw new NotSupportedException();

            public void FadeOff() => throw new NotSupportedException();

            public void FadeOn() => throw new NotSupportedException();

            public void FadeOut() => throw new NotSupportedException();

            public void GoTo(int fieldId, int walkmeshId) => throw new NotSupportedException();

            public void PrepareGoTo(int fieldId) => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class GameplayServiceId : ServiceId<IGameplayService>, IGameplayService
        {
            #region Properties

            public bool IsRandomBattlesEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public bool IsUserControlEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            #endregion Properties

            #region Methods

            public void ResetAllData() => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class GlobalVariableServiceId : ServiceId<IGlobalVariableService>, IGlobalVariableService
        {
            #region Methods

            public T Get<T>(GlobalVariableId<T> id) where T : unmanaged => throw new NotSupportedException();

            public void Set<T>(GlobalVariableId<T> id, T value) where T : unmanaged => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class InteractionServiceId : ServiceId<IInteractionService>, IInteractionService
        {
            #region Indexers

            public int this[ScriptResultId id]
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            #endregion Indexers

            #region Methods

            public IAwaitable Wait(int frameNumber) => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class MenuServiceId : ServiceId<IMenuService>, IMenuService
        {
            #region Methods

            public IAwaitable ShowEnterNameDialog(NamedEntity entity) => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class MessageServiceId : ServiceId<IMessageService>, IMessageService
        {
            #region Methods

            public void Close(int channel) => throw new NotSupportedException();

            public void Show(int channel, int messageId) => throw new NotSupportedException();

            public void Show(int channel, int messageId, int posX, int posY) => throw new NotSupportedException();

            public IAwaitable ShowDialog(int channel, int messageId, int posX, int posY) => throw new NotSupportedException();

            public IAwaitable ShowQuestion(int channel, int messageId, int firstLine, int lastLine, int beginLine, int cancelLine) => throw new NotSupportedException();

            public IAwaitable ShowQuestion(int channel, int messageId, int firstLine, int lastLine, int beginLine, int cancelLine, int posX, int posY) => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class MovieServiceId : ServiceId<IMovieService>, IMovieService
        {
            #region Methods

            public void Play() => throw new NotSupportedException();

            public void PrepareToPlay(int movieId, bool flag) => throw new NotSupportedException();

            public void Wait() => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class MusicServiceId : ServiceId<IMusicService>, IMusicService
        {
            #region Methods

            public void ChangeBattleMusic(MusicId musicId) => throw new NotSupportedException();

            public void ChangeMusicVolume(int volume, bool flag) => throw new NotSupportedException();

            public void ChangeMusicVolume(int volume, bool flag, int transitionDuration) => throw new NotSupportedException();

            public void LoadFieldMusic(MusicId musicId) => throw new NotSupportedException();

            public void PlayFieldMusic() => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class PartyServiceId : ServiceId<IPartyService>, IPartyService
        {
            #region Properties

            public bool IsPartySwitchEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            #endregion Properties

            #region Methods

            public void AddPartyCharacter(Characters characterId) => throw new NotSupportedException();

            public void AddPlayableCharacter(Characters characterId) => throw new NotSupportedException();

            public void ChangeCharacterState(Characters characterId, bool isSwitchable, bool isSelectable) => throw new NotSupportedException();

            public void ChangeParty(Characters characterId1, Characters characterId2, Characters characterId3) => throw new NotSupportedException();

            public FieldObject FindPartyCharacterObject(int partyId) => throw new NotSupportedException();

            public void RemovePartyCharacter(Characters characterId) => throw new NotSupportedException();

            public void RemovePlayableCharacter(Characters characterId) => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class RenderingServiceId : ServiceId<IRenderingService>, IRenderingService
        {
            #region Properties

            public int BackgroundFPS
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            #endregion Properties

            #region Methods

            public void AddScreenColor(Color Color) => throw new NotSupportedException();

            public void AddScreenColorTransition(Color Color, Color offset, int transitionDuration) => throw new NotSupportedException();

            public void AnimateBackground(int firstFrame, int lastFrame) => throw new NotSupportedException();

            public void DrawBackground() => throw new NotSupportedException();

            public void SubScreenColor(Color Color) => throw new NotSupportedException();

            public void SubScreenColorTransition(Color Color, Color offset, int transitionDuration) => throw new NotSupportedException();

            public IAwaitable Wait() => throw new NotSupportedException();

            #endregion Methods
        }

        private sealed class SalaryServiceId : ServiceId<ISalaryService>, ISalaryService
        {
            #region Properties

            public bool IsSalaryAlertEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public bool IsSalaryEnabled
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            #endregion Properties
        }

        private sealed class SoundServiceId : ServiceId<ISoundService>, ISoundService
        {
            #region Methods

            public void PlaySound(int fieldSoundIndex, int pan, int volume, int channel) => throw new NotSupportedException();

            #endregion Methods
        }

        #endregion Classes
    }

    public abstract class ServiceId<T>
    {
        #region Properties

        public bool IsSupported => false;

        #endregion Properties

        #region Indexers

        public T this[IServices services] => services.Service(this);

        #endregion Indexers
    }
}