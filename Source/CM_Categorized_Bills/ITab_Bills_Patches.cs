using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Categorized_Bills
{
    [StaticConstructorOnStartup]
    public static class ITab_Bills_Patches
    {
        [HarmonyPatch(typeof(ITab_Bills))]
        [HarmonyPatch("FillTab", MethodType.Normal)]
        public static class ITab_Bills_FillTab
        {
            [HarmonyPostfix]
            public static void Postfix()
            {

                Rect areaRect = new Rect(160f, 10f, 29f, 29f);
                Rect buttonRect = new Rect(0, 0f, 29f, 29f);
                GUI.BeginGroup(areaRect);
                if (Widgets.ButtonText(buttonRect, "C"))
                {
                    Find.WindowStack.Add(new FloatMenu(RecipeOptionsMakerMaker()()));
                }
                GUI.EndGroup();
            }

            //[HarmonyTranspiler]
            //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            //{
            //    MethodInfo recipeOptionsMaker = AccessTools.Method(typeof(ITab_Bills_FillTab), nameof(ITab_Bills_FillTab.RecipeOptionsMakerMaker));

            //    List<CodeInstruction> instructionList = instructions.ToList();

            //    for (int i = 1; i < instructionList.Count - 1; ++i)
            //    {
            //        if (instructionList[i].opcode == OpCodes.Ldftn && (instructionList[i].operand as MethodInfo)?.ReturnType == typeof(List<FloatMenuOption>) && instructionList[i - 1].IsLdarg())
            //        {
            //            Log.Message("[CM_Categorized_Bills] - patching in categorized bill selection.");

            //            instructionList[i - 1] = new CodeInstruction(OpCodes.Nop);
            //            instructionList[i - 0] = new CodeInstruction(OpCodes.Nop);
            //            instructionList[i + 1] = new CodeInstruction(OpCodes.Call, recipeOptionsMaker);

            //            break;
            //        }
            //    }

            //    foreach (CodeInstruction instruction in instructionList)
            //    {
            //        yield return instruction;
            //    }
            //}

            public static Func<List<FloatMenuOption>> RecipeOptionsMakerMaker()
            {
                //// Original function for reference
                //
                // 	Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
                // 	{
                // 		List<FloatMenuOption> list = new List<FloatMenuOption>();
                // 		RecipeDef recipe = default(RecipeDef);
                // 		for (int i = 0; i < SelTable.def.AllRecipes.Count; i++)
                // 		{
                // 			if (SelTable.def.AllRecipes[i].AvailableNow && SelTable.def.AllRecipes[i].AvailableOnNow(SelTable))
                // 			{
                // 				recipe = SelTable.def.AllRecipes[i];
                // 				list.Add(new FloatMenuOption(recipe.LabelCap, delegate
                // 				{
                // 					if (!SelTable.Map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                // 					{
                // 						Bill.CreateNoPawnsWithSkillDialog(recipe);
                // 					}
                // 					Bill bill2 = recipe.MakeNewBill();
                // 					SelTable.billStack.AddBill(bill2);
                // 					if (recipe.conceptLearned != null)
                // 					{
                // 						PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                // 					}
                // 					if (TutorSystem.TutorialMode)
                // 					{
                // 						TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap.Resolve());
                // 					}
                // 				}, recipe.UIIconThing, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe)));
                // 			}
                // 		}
                // 		if (!list.Any())
                // 		{
                // 			list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                // 		}
                // 		return list;
                // 	};

                Building_WorkTable SelTable = (Building_WorkTable)Find.Selector.SingleSelectedThing;

                List<RecipeDef> genericRecipes = new List<RecipeDef>();
                Dictionary<ThingCategoryDef, List<RecipeDef>> recipesByCategory = new Dictionary<ThingCategoryDef, List<RecipeDef>>();

                List<ThingCategoryDef> validCategories = new List<ThingCategoryDef>();

                Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
                {
                    // We'll start by sorting all of the recipes into lists by category
                    foreach (RecipeDef recipeDef in SelTable.def.AllRecipes)
                    {
                        if (!recipeDef.AvailableNow || !recipeDef.AvailableOnNow(SelTable))
                            continue;

                        List<RecipeDef> recipeList = genericRecipes;
                        bool added = false;

                        // Check each product in the recipe
                        foreach (ThingDefCountClass product in recipeDef.products)
                        {
                            if (product != null && product.thingDef != null && product.thingDef.thingCategories != null && product.thingDef.thingCategories.Count > 0)
                            {
                                // Check each category the product belongs to and track add the recipe to it in our local tracker
                                foreach (ThingCategoryDef thingCategory in product.thingDef.thingCategories)
                                {
                                    if (recipesByCategory.ContainsKey(thingCategory))
                                    {
                                        recipeList = recipesByCategory[thingCategory];
                                    }
                                    else
                                    {
                                        recipeList = new List<RecipeDef>();
                                        recipesByCategory.Add(thingCategory, recipeList);
                                    }

                                    if (!recipeList.Contains(recipeDef))
                                    {
                                        // Also put this and all parent categories in a big list so we know to include them in the sublists we show
                                        validCategories.Add(thingCategory);
                                        validCategories.AddRange(thingCategory.Parents);

                                        recipeList.Add(recipeDef);
                                    }
                                    added = true;
                                }
                            }
                        }

                        // If it didn't have a category, we'll put it in the highest level list
                        if (!added)
                        {
                            genericRecipes.Add(recipeDef);
                        }

                        validCategories = validCategories.Distinct().ToList();
                    }

                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    // First the uncategorized recipes
                    foreach (RecipeDef recipe in genericRecipes)
                    {
                        options.Add(RecipeFloatMenuOption(SelTable, recipe));
                    }

                    // Now add the categorized lists
                    if (validCategories.Contains(ThingCategoryDefOf.Root))
                    {
                        // If there is only one subcategory with entries, skip right to it
                        if (!options.Any() && recipesByCategory.Keys.Count == 1)
                        {

                            options.AddRange(RecipeSubList(SelTable, recipesByCategory.Keys.First(), validCategories, recipesByCategory));

                        }
                        else
                        {
                            List<ThingCategoryDef> validChildCategories = GetValidChildCategories(ThingCategoryDefOf.Root, validCategories);

                            // If have no recipes at this level and only one subcategory, lets condense
                            if (!options.Any() && validChildCategories.Count == 1)
                            {
                                options.AddRange(RecipeSubList(SelTable, validChildCategories[0], validCategories, recipesByCategory));
                            }
                            // Otherwise, build a tree
                            else
                            {
                                options.AddRange(RecipeSubList(SelTable, ThingCategoryDefOf.Root, validCategories, recipesByCategory));
                            }
                        }
                    }

                    if (!options.Any())
                    {
                        options.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
                    }

                    return options;
                };

                return recipeOptionsMaker;
            }

            public static List<ThingCategoryDef> GetValidChildCategories(ThingCategoryDef categoryDef, List<ThingCategoryDef> validCategories)
            {
                if (categoryDef.childCategories == null)
                    return new List<ThingCategoryDef>();

                return categoryDef.childCategories.Where(category => validCategories.Contains(category)).ToList();
            }

            public static List<FloatMenuOption> RecipeSubList(Building_WorkTable SelTable, ThingCategoryDef categoryDef, List<ThingCategoryDef> validCategories, Dictionary<ThingCategoryDef, List<RecipeDef>> recipesByCategory, string condensedLabel = "")
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                // Recipes at this level
                if (recipesByCategory.ContainsKey(categoryDef))
                {
                    foreach (RecipeDef recipeDef in recipesByCategory[categoryDef])
                    {
                        options.Add(RecipeFloatMenuOption(SelTable, recipeDef));
                    }
                }

                // Subcategories
                foreach (ThingCategoryDef childCategory in categoryDef.childCategories)
                {
                    if (validCategories.Contains(childCategory))
                    {
                        options.AddRange(RecipeCategoryFloatMenuOptions(SelTable, childCategory, validCategories, recipesByCategory));
                    }
                }

                return options;
            }

            public static List<FloatMenuOption> RecipeCategoryFloatMenuOptions(Building_WorkTable SelTable, ThingCategoryDef categoryDef, List<ThingCategoryDef> validCategories, Dictionary<ThingCategoryDef, List<RecipeDef>> recipesByCategory, string condensedLabel = "")
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                string label = categoryDef.LabelCap;
                if (!string.IsNullOrEmpty(condensedLabel))
                    label = condensedLabel + " > " + label;

                // If this category has no recipes of its own, lets condense it down to save some hassle
                if (!recipesByCategory.ContainsKey(categoryDef))
                {
                    foreach (ThingCategoryDef childCategory in categoryDef.childCategories)
                    {
                        if (validCategories.Contains(childCategory))
                        {
                             options.AddRange(RecipeCategoryFloatMenuOptions(SelTable, childCategory, validCategories, recipesByCategory, label));
                        }
                    }
                }
                else
                {
                    options.Add(new FloatMenuOption(" > " + label, delegate
                    {
                        Find.WindowStack.Add(new FloatMenu(RecipeSubList(SelTable, categoryDef, validCategories, recipesByCategory)));
                    }, MenuOptionPriority.Default, null, null, 29f));
                }

                return options;
            }

            public static FloatMenuOption RecipeFloatMenuOption(Building_WorkTable SelTable, RecipeDef recipe)
            {
                return new FloatMenuOption(recipe.LabelCap, delegate
                {
                    if (!SelTable.Map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                    {
                        Bill.CreateNoPawnsWithSkillDialog(recipe);
                    }
                    Bill bill2 = recipe.MakeNewBill();
                    SelTable.billStack.AddBill(bill2);
                    if (recipe.conceptLearned != null)
                    {
                        PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                    }
                    if (TutorSystem.TutorialMode)
                    {
                        TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap.Resolve());
                    }
                }, recipe.UIIconThing, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe));
            }
        }
    }
}
