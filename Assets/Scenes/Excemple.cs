using UnityEngine;
using GameFrameWork.Config;
using UnityEngine.UI;

public class Excemple : MonoBehaviour
{
    void Start()
    {
        Transform nameAll = transform.Find("NameAll");
        Text name = transform.Find("Name").GetComponent<Text>();
        Text icon = transform.Find("Icon").GetComponent<Text>();
        Text type = transform.Find("Type").GetComponent<Text>();
        Text role = transform.Find("Role").GetComponent<Text>();
        string _nameAll = null;
        //循环出表中所有的名字字段
        foreach (var item in SkillCfgCtrl.Instance.All)
        {
            _nameAll += item.Value.Name + ",";
        }
        nameAll.GetComponent<Text>().text = _nameAll;

        //传入一个id得到一个对象
        SkillCfgItem skillCfgItem = SkillCfgCtrl.TryGet(1001);
        name.text = skillCfgItem.Name;
        icon.text = skillCfgItem.Icon;
        type.text = skillCfgItem.Type;
        role.text = skillCfgItem.Role;
    }
}
