/* 模版用于匹配导出的CSharp脚本工具 */
using LitJson;
using System.IO;
using System.Collections.Generic;

namespace GameFrameWork.Config
{
    public sealed class SkillCfgItem
    {
        /// <summary> 
 		/// ID
 		/// </summary> 
 		public int id;
		/// <summary> 
 		/// 技能名字
 		/// </summary> 
 		public string Name;
		/// <summary> 
 		/// 技能图标
 		/// </summary> 
 		public string Icon;
		/// <summary> 
 		/// 使用角色
 		/// </summary> 
 		public string Role;
		/// <summary> 
 		/// 技能类型
 		/// </summary> 
 		public string Type;
		/// <summary> 
 		/// 技能顺序
 		/// </summary> 
 		public string Order;
		/// <summary> 
 		/// 冷却时间
 		/// </summary> 
 		public int CoolTime;
		/// <summary> 
 		/// 伤害
 		/// </summary> 
 		public int Damage;
    }

    public class SkillCfgCtrl
    {
        Dictionary<int, SkillCfgItem> mConfigDic;
        protected SkillCfgCtrl()
        {
            mConfigDic = new Dictionary<int, SkillCfgItem>();
			if (mConfigDic.Count == 0)
            {
                string mPathJson = @"Assets\_Config\JsonData\";
                using (StreamReader sr = new StreamReader(mPathJson + GetAssetName()))
                {
                    LoadConfig(sr.ReadToEnd());
                }

            }
        }

        class SingletonCreator
        {
            static SingletonCreator() { }
            internal static readonly SkillCfgCtrl instance = new SkillCfgCtrl();
        }

        public static SkillCfgCtrl Instance
        {
            get { return SingletonCreator.instance; }
        }
		
		public static SkillCfgItem TryGet(int id)
        {
            return Instance.Get(id);
        }

        public string GetAssetName()
        {
        	return "Skill.json";
        }

        public Dictionary<int, SkillCfgItem> All
        {
            get { return mConfigDic; }
        }

        public void LoadConfig(string content)
        {
            mConfigDic.Clear();
            JsonData data = JsonMapper.ToObject(content);
            for (int i = 0; i < data.Count; ++i)
            {
                var obj = JsonMapper.ToObject<SkillCfgItem>(data[i].ToJson());
                if (obj != null)
                    mConfigDic.Add(obj.id, obj);
            }
        }

        public SkillCfgItem Get(int id)
        {
            if (mConfigDic.ContainsKey(id))
                return mConfigDic[id];
            return null;
        }
    }
}
