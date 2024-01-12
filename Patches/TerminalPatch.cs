using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace LCFlashCommandFix.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    public static class TerminalPatch
    {
        private static readonly FieldInfo specialNodes = typeof(TerminalNodesList).GetField(nameof(TerminalNodesList.specialNodes));
        [HarmonyPatch("ParsePlayerSentence")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> flashCommandFix(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, typeof(TransformAndName).GetField(nameof(TransformAndName.isNonPlayer)))
                );

            CodeInstruction jump = codeMatcher.MatchForward(true, new CodeMatch(OpCodes.Brfalse)).Instruction;

            Label label = (Label) jump.operand;

            codeMatcher = codeMatcher.MatchForward(true,
                new CodeMatch(instruction => instruction.opcode == OpCodes.Ldarg_0 && instruction.labels.Any(l => l.Equals(label)))
            );

            codeMatcher.Instruction.labels = codeMatcher.Instruction.labels.Except(new List<Label> { label }).ToList();

            return codeMatcher
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0).WithLabels(label),
                    new CodeInstruction(OpCodes.Ldfld, typeof(Terminal).GetField(nameof(Terminal.terminalNodes))),
                    new CodeInstruction(OpCodes.Ldfld, specialNodes),
                    new CodeInstruction(OpCodes.Ldc_I4_S, 10),
                    new CodeInstruction(OpCodes.Callvirt, getListGetterMethod(instructions)),
                    new CodeInstruction(OpCodes.Ret)
                )
                .InstructionEnumeration();
        }

        private static MethodInfo getListGetterMethod(IEnumerable<CodeInstruction> codes)
        {
            CodeInstruction getCall = codes.First(
                i => i.opcode == OpCodes.Callvirt &&
                     i.operand is MethodInfo &&
                     (i.operand as MethodInfo).ReturnType == typeof(TerminalNode) &&
                     (i.operand as MethodInfo).Name == "get_Item" &&
                     (i.operand as MethodInfo).GetParameters().Count() == 1 &&
                     (i.operand as MethodInfo).GetParameters().ElementAt(0).ParameterType == typeof(Int32) &&
                     (i.operand as MethodInfo).DeclaringType == specialNodes.FieldType
                 );

            return getCall == null ? null : getCall.operand as MethodInfo;
        }
    }
}
