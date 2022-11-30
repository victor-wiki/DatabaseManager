using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Helper
{
    public static class DbObjectsTreeHelper
    {
        public static readonly string FakeNodeName = "_FakeNode_";
        public static DatabaseObjectType DefaultObjectType = DatabaseObjectType.Type | DatabaseObjectType.Sequence | DatabaseObjectType.Table
                                                           | DatabaseObjectType.View | DatabaseObjectType.Procedure | DatabaseObjectType.Function | DatabaseObjectType.Trigger;

        public static string GetFolderNameByDbObjectType(DatabaseObjectType databaseObjectType)
        {
            return ManagerUtil.GetPluralString(databaseObjectType.ToString());
        }

        public static string GetFolderNameByDbObjectType(Type objType)
        {
            return ManagerUtil.GetPluralString(objType.Name.ToString());
        }

        public static DatabaseObjectType GetDbObjectTypeByFolderName(string folderName)
        {
            if (folderName == DbObjectTreeFolderType.Types.ToString())
            {
                return DatabaseObjectType.Type;
            }

            string value = ManagerUtil.GetSingularString(folderName);
            DatabaseObjectType type = DatabaseObjectType.None;

            Enum.TryParse(value, out type);

            return type;
        }

        public static string GetImageKey(string name)
        {
            return $"tree_{name}.png";
        }

        public static TreeNode CreateTreeNode(string name, string text, string imageKeyName)
        {
            TreeNode node = new TreeNode(text);
            node.Name = name;
            node.ImageKey = GetImageKey(imageKeyName);
            node.SelectedImageKey = node.ImageKey;
            return node;
        }

        public static TreeNode CreateFolderNode(string name, string text, bool createFakeNode = false)
        {
            TreeNode node = CreateTreeNode(name, text, "Folder");

            if (createFakeNode)
            {
                node.Nodes.Add(CreateFakeNode());
            }

            return node;
        }

        public static TreeNode CreateFakeNode()
        {
            return CreateTreeNode(FakeNodeName, "", "Fake");
        }

        #region TreeNode Extension
        public static TreeNode CreateTreeNode<T>(T dbObject, bool createFakeNode = false)
            where T : DatabaseObject
        {
            TreeNode node = CreateTreeNode(dbObject.Name, dbObject.Name, dbObject.GetType().Name);
            node.Tag = dbObject;

            if (createFakeNode)
            {
                node.Nodes.Add(CreateFakeNode());
            }

            return node;
        }

        public static TreeNode AddDbObjectNodes<T>(this TreeNode treeNode, IEnumerable<T> dbObjects)
         where T : DatabaseObject
        {
            treeNode.Nodes.AddRange(CreateDbObjectNodes(dbObjects).ToArray());

            return treeNode;
        }

        public static TreeNode AddDbObjectFolderNode<T>(this TreeNode treeNode, IEnumerable<T> dbObjects)
           where T : DatabaseObject
        {
            string folderName = GetFolderNameByDbObjectType(typeof(T));

            TreeNode node = CreateFolderNode(folderName, folderName, dbObjects);
            if (node != null)
            {
                treeNode.Nodes.Add(node);
                return node;
            }

            return null;
        }

        public static TreeNode AddDbObjectFolderNode<T>(this TreeNodeCollection treeNodes, string name, string text, List<T> dbObjects)
          where T : DatabaseObject
        {
            TreeNode node = CreateFolderNode(name, text, dbObjects);

            if (node != null)
            {
                treeNodes.Add(node);

                return node;
            }

            return null;
        }

        public static TreeNode CreateFolderNode<T>(string name, string text, IEnumerable<T> dbObjects)
           where T : DatabaseObject
        {
            if (dbObjects.Count() > 0)
            {
                var node = CreateFolderNode(name, text);

                node.Nodes.AddRange(CreateDbObjectNodes(dbObjects).ToArray());

                return node;
            }
            return null;
        }

        public static IEnumerable<TreeNode> CreateDbObjectNodes<T>(IEnumerable<T> dbObjects, bool alwaysShowSchema = false)
           where T : DatabaseObject
        {
            bool isUniqueDbSchema = dbObjects.GroupBy(item => item.Schema).Count() == 1;

            if (!isUniqueDbSchema)
            {
                dbObjects = dbObjects.OrderBy(item => item.Schema).ThenBy(item => item.Name);
            }

            foreach (var dbObj in dbObjects)
            {
                string text = (alwaysShowSchema || !isUniqueDbSchema) ? $"{dbObj.Schema}.{dbObj.Name}" : dbObj.Name;

                string imgKeyName = typeof(T).Name;              

                if (dbObj is Function func)
                {
                    if (func.IsTriggerFunction)
                    {
                        imgKeyName = $"{imgKeyName}_Trigger";
                    }
                }

                TreeNode node = CreateTreeNode(dbObj.Name, text, imgKeyName);               
                node.Tag = dbObj;                

                yield return node;
            }
        }
        #endregion
    }
}