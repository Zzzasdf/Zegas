using UniFramework.Event;

public class SceneEventDefine
{
    public class ChangeToHomeScene : IEventMessage
    {
        public static void SendEventMessage(string hotUpdateFirstScene)
        {
            var msg = new ChangeToHomeScene(hotUpdateFirstScene);
            UniEvent.SendMessage(msg);
        }
        
        public string HotUpdateFirstScene { get; }

        private ChangeToHomeScene(string hotUpdateFirstScene)
        {
            HotUpdateFirstScene = hotUpdateFirstScene;
        }
    }

    public class ChangeToBattleScene : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ChangeToBattleScene();
            UniEvent.SendMessage(msg);
        }
    }
}