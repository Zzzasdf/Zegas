using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MacroDefinerModule
{
    public enum EMacroDefiner
    {
        UNITASK_DOTWEEN_SUPPORT = 1,

        FRAME_ERROR = 10001,
        FRAME_WARNING = 10002,
        FRAME_LOG = 10003,

        ERROR = 11001,
        WARNING = 11002,
        LOG = 11003,
            
        POOLED_EXCEPTION = 12001,
    }
    
    public sealed partial class MacroDefinerWindow
    {
        public static readonly ReadOnlyDictionary<EMacroDefiner, DefineNode> NodeDict = new ReadOnlyDictionary<EMacroDefiner, DefineNode>
        (
            new Dictionary<EMacroDefiner, DefineNode>
            {
                [EMacroDefiner.UNITASK_DOTWEEN_SUPPORT] = new DefineNode(EMacroDefiner.UNITASK_DOTWEEN_SUPPORT, "UniTask Tween  支持"),

                [EMacroDefiner.FRAME_ERROR] = new DefineNode(EMacroDefiner.FRAME_ERROR, "框架_Error"),
                [EMacroDefiner.FRAME_WARNING] = new DefineNode(EMacroDefiner.FRAME_WARNING, "框架_Warning"),
                [EMacroDefiner.FRAME_LOG] = new DefineNode(EMacroDefiner.FRAME_LOG, "框架_Log"),

                [EMacroDefiner.ERROR] = new DefineNode(EMacroDefiner.ERROR, "Error"),
                [EMacroDefiner.WARNING] = new DefineNode(EMacroDefiner.WARNING, "Warning"),
                [EMacroDefiner.LOG] = new DefineNode(EMacroDefiner.LOG, "Log"),

                [EMacroDefiner.POOLED_EXCEPTION] = new DefineNode(EMacroDefiner.POOLED_EXCEPTION, "池化对象异常"),
            }
        );
        
        // 初始化关系链
        private void InitRelationshipChain()
        {
            EMacroDefiner.FRAME_ERROR.Linker(
                EMacroDefiner.FRAME_WARNING.Linker(
                    EMacroDefiner.FRAME_LOG));
            
            EMacroDefiner.ERROR.Linker(
                EMacroDefiner.WARNING.Linker(
                    EMacroDefiner.LOG));
        }

        /// 固定宏
        private List<EMacroDefiner> fixedDefines = new List<EMacroDefiner>
        {
            EMacroDefiner.UNITASK_DOTWEEN_SUPPORT,
        };
        // 可选宏
        private List<EMacroDefiner> optionalDefines = new List<EMacroDefiner>
        {
            EMacroDefiner.FRAME_ERROR,
            EMacroDefiner.ERROR,
            EMacroDefiner.POOLED_EXCEPTION,
        };
        
        public enum EMode
        {
            Development = 1,
            Releases = 2,
        }

        private readonly ReadOnlyDictionary<EMode, (string modeName, List<EMacroDefiner> eMacroDefiners)> modeDict = new ReadOnlyDictionary<EMode, (string modeName, List<EMacroDefiner>)>
        (
            new Dictionary<EMode, (string modeName, List<EMacroDefiner>)>
            {
                [EMode.Development] = ("开发模式", new List<EMacroDefiner>
                {
                    EMacroDefiner.UNITASK_DOTWEEN_SUPPORT,
                    EMacroDefiner.FRAME_LOG,
                    EMacroDefiner.LOG,
                    EMacroDefiner.POOLED_EXCEPTION,
                }),
                [EMode.Releases] = ("发布模式", new List<EMacroDefiner>
                {
                    EMacroDefiner.UNITASK_DOTWEEN_SUPPORT,
                    EMacroDefiner.FRAME_ERROR,
                    EMacroDefiner.ERROR,
                }),
            }
        );
    }

    public static class EMacroDefinerExtensions
    {
        public static EMacroDefiner Linker(this EMacroDefiner self, params EMacroDefiner[] children)
        {
            DefineNode selfNode = MacroDefinerWindow.NodeDict[self];
            for (int i = 0; i < children.Length; i++)
            {
                EMacroDefiner child = children[i];
                DefineNode childNode = MacroDefinerWindow.NodeDict[child];
                selfNode.AddChild(child);
                childNode.AddParent(self);
            }
            return self;
        }
    }
}