//命名空间，相当于给代码分类文件夹，表示这段代码属于游戏棋盘（Board）逻辑层。
namespace Game.Board
{
    //合成系统类，提供物品合成的逻辑判断和计算
    public static class MergeSystem
    {
        public const int MaxLevel = 3;//定义合成的最大等级为 3 级

        //判断两个物品能否合成 (CanMerge)
        public static bool CanMerge(int levelA, int levelB)
        {
            if (levelA <= 0 || levelB <= 0)
                return false;
            if (levelA >= MaxLevel || levelB >= MaxLevel)
                return false;
            return levelA == levelB;
        }

        //判断是否已达到满级(IsMaxLevel)
        public static bool IsMaxLevel(int level)
        {
            return level >= MaxLevel;
        }

        //计算合成后的新等级(GetMergeResultLevel)
        public static int GetMergeResultLevel(int level)
        {
            if (level >= MaxLevel)
                return MaxLevel;
            return level + 1;
        }
    }
}
