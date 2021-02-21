//using System.Collections.Generic;
//using System.Linq;

//using HarmonyLib;
//using RimWorld;
//using Verse;

//namespace CM_Categorized_Bills
//{
//    public class RecipeCategoryTree
//    {
//        public RecipeCategoryNode root = new RecipeCategoryNode(null);

//        public void AddRecipe(ThingCategoryDef category, RecipeDef recipe)
//        {
//            if (category == null)
//            {
//                root.recipes.Add(recipe);
//                return;
//            }

//            RecipeCategoryNode currentNode = root;
//            List<ThingCategoryDef> categoryNodeList = category.Parents.Reverse().ToList();

//            foreach(ThingCategoryDef categoryDef in categoryNodeList)
//            {
//                RecipeCategoryNode nextNode = currentNode.children.Find(node => node.category == categoryDef);
//                if (nextNode == null)
//                {
//                    currentNode.children.Add(new RecipeCategoryNode(categoryDef));
//                    currentNode.children.sort
//                }
//            }
//        }
//    }

//    public class RecipeCategoryNode
//    {
//        public ThingCategoryDef category;
//        public List<RecipeDef> recipes = new List<RecipeDef>();
//        public List<RecipeCategoryNode> children = new List<RecipeCategoryNode>();

//        public RecipeCategoryNode(ThingCategoryDef categoryDef)
//        {
//            category = categoryDef;
//        }
//    }
//}
