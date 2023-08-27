using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace ForceDirectedLibDemo.Tools
{
	public static class ManagedMemoryTools
	{
		//http://www.abstractpath.com/2009/memcpy-in-nWindow/
		//http://coding.grax.com/2013/06/fast-array-fill-function-revisited.html

		//################################################################################
		//################################################################################

		public delegate void FadeDelegate(IntPtr oTarget, int nCount, float nMultiplier);
		public delegate void SetPixelDelegate(IntPtr oTarget, int nPixelIndex, int nARGB);
		public delegate void SetInt32AddressDelegate(int nAddress, int nValue);

		public delegate void MemoryCopyDelegate(IntPtr oDst, IntPtr oSrc, int nCount);
		public delegate void MemorySetDelegate(IntPtr oDst, int nValue, int nCount);
		public delegate void MemorySetInt32Delegate(IntPtr oDst, int nValue);

		//################################################################################
		//################################################################################

		public static readonly FadeDelegate FadeMulDYN;
		public static readonly SetPixelDelegate SetPixelDYN;
		public static readonly SetInt32AddressDelegate SetInt32AddressDYN;

		public static readonly MemoryCopyDelegate MemoryCopy;
		public static readonly MemorySetDelegate MemorySet;
		public static readonly MemorySetDelegate MemorySet32Dyn;
		public static readonly MemorySetInt32Delegate MemSetInt32;

		//################################################################################
		//################################################################################

		static ManagedMemoryTools()
		{
			//########################################
			//########################################

			#region SetInt32AddressDYN

			{
				var oDynamicMethod = new DynamicMethod
				(
					"SetInt32AddressDYN",
					typeof(void),
					new[] { typeof(int), typeof(int) },
					typeof(ManagedMemoryTools)
				);

				ILGenerator oILGenerator = oDynamicMethod.GetILGenerator();

				oILGenerator.Emit(OpCodes.Ldarg_0);
				oILGenerator.Emit(OpCodes.Ldarg_1);
				oILGenerator.Emit(OpCodes.Stind_I4);
				oILGenerator.Emit(OpCodes.Ret);

				//

				SetInt32AddressDYN = (SetInt32AddressDelegate)oDynamicMethod.CreateDelegate(typeof(SetInt32AddressDelegate));
			}

			#endregion

			//########################################
			//########################################

			#region SetPixelDYN

			{
				Type type = typeof(IntPtr);
				BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				MethodInfo toPointer = type.GetMethod("ToPointer", bf, null, Array.Empty<Type>(), null)!;

				var oDynamicMethod = new DynamicMethod
				(
					"SetPixelDYN",
					typeof(void),
					new[] { typeof(IntPtr), typeof(int), typeof(int) },
					typeof(ManagedMemoryTools)
				);

				ILGenerator oILGenerator = oDynamicMethod.GetILGenerator();

				oILGenerator.Emit(OpCodes.Ldarga_S, 0);//oBackBuffer
				oILGenerator.Emit(OpCodes.Call, toPointer);
				oILGenerator.Emit(OpCodes.Ldarg_1);//nIndex
				oILGenerator.Emit(OpCodes.Conv_I);
				oILGenerator.Emit(OpCodes.Ldc_I4_4);
				oILGenerator.Emit(OpCodes.Mul);
				oILGenerator.Emit(OpCodes.Add);
				oILGenerator.Emit(OpCodes.Ldarg_2);//nValue
				oILGenerator.Emit(OpCodes.Stind_I4);

				oILGenerator.Emit(OpCodes.Ret);

				//

				SetPixelDYN = (SetPixelDelegate)oDynamicMethod.CreateDelegate(typeof(SetPixelDelegate));
			}

			#endregion

			//########################################
			//########################################

			#region FadeMulDYN

			{
				MethodInfo oToPointer = typeof(IntPtr).GetMethod("ToPointer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null)!;

				var oDynamicMethod = new DynamicMethod
				(
					"FadeMulDYN",
					typeof(void),
					new[] { typeof(IntPtr), typeof(int), typeof(float) },
					typeof(ManagedMemoryTools)
				);

				ILGenerator oILGenerator = oDynamicMethod.GetILGenerator();

				LocalBuilder pTarget = oILGenerator.DeclareLocal(typeof(Int32*));
				LocalBuilder pValue = oILGenerator.DeclareLocal(typeof(Byte*));
				LocalBuilder pEnd = oILGenerator.DeclareLocal(typeof(Byte*));

				Label oWhileLabel = oILGenerator.DefineLabel();
				Label oLoopLabel = oILGenerator.DefineLabel();

				//int* pTarget = (int*)oTarget.ToPointer();
				oILGenerator.Emit(OpCodes.Ldarga_S, 0);//oTarget
				oILGenerator.Emit(OpCodes.Call, oToPointer);
				oILGenerator.Emit(OpCodes.Stloc, pTarget);

				//byte* pValue = (byte*)(pTarget);
				oILGenerator.Emit(OpCodes.Ldloc, pTarget);
				oILGenerator.Emit(OpCodes.Stloc, pValue);

				//byte* pEnd = (byte*)(pTarget + nCount);
				oILGenerator.Emit(OpCodes.Ldloc, pTarget);
				oILGenerator.Emit(OpCodes.Ldarg_1);//nCount
				oILGenerator.Emit(OpCodes.Ldc_I4_4);
				oILGenerator.Emit(OpCodes.Mul);
				oILGenerator.Emit(OpCodes.Add);
				oILGenerator.Emit(OpCodes.Stloc, pEnd);

				oILGenerator.Emit(OpCodes.Br_S, oWhileLabel);

				oILGenerator.MarkLabel(oLoopLabel);

				//pValue[0] = (byte)(pValue[0] * nMultiplier);
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldind_U1);
				oILGenerator.Emit(OpCodes.Conv_R4);//(float)
				oILGenerator.Emit(OpCodes.Ldarg_2);//nMultiplier
				oILGenerator.Emit(OpCodes.Mul);
				oILGenerator.Emit(OpCodes.Conv_U1);
				oILGenerator.Emit(OpCodes.Stind_I1);

				//pValue[1] = (byte)(pValue[1] * nMultiplier);
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldc_I4_1);//1
				oILGenerator.Emit(OpCodes.Add);
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldc_I4_1);//1
				oILGenerator.Emit(OpCodes.Add);
				oILGenerator.Emit(OpCodes.Ldind_U1);
				oILGenerator.Emit(OpCodes.Conv_R4);//(float)
				oILGenerator.Emit(OpCodes.Ldarg_2);//nMultiplier
				oILGenerator.Emit(OpCodes.Mul);
				oILGenerator.Emit(OpCodes.Conv_U1);
				oILGenerator.Emit(OpCodes.Stind_I1);

				//pValue[2] = (byte)(pValue[2] * nMultiplier);
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldc_I4_2);//2
				oILGenerator.Emit(OpCodes.Add);
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldc_I4_2);//2
				oILGenerator.Emit(OpCodes.Add);
				oILGenerator.Emit(OpCodes.Ldind_U1);
				oILGenerator.Emit(OpCodes.Conv_R4);//(float)
				oILGenerator.Emit(OpCodes.Ldarg_2);//nMultiplier
				oILGenerator.Emit(OpCodes.Mul);
				oILGenerator.Emit(OpCodes.Conv_U1);
				oILGenerator.Emit(OpCodes.Stind_I1);

				//pValue += 4;
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldc_I4_4);
				oILGenerator.Emit(OpCodes.Add);
				oILGenerator.Emit(OpCodes.Stloc, pValue);

				oILGenerator.MarkLabel(oWhileLabel);

				//while (pValue < pEnd)
				oILGenerator.Emit(OpCodes.Ldloc, pValue);
				oILGenerator.Emit(OpCodes.Ldloc, pEnd);
				oILGenerator.Emit(OpCodes.Blt_Un_S, oLoopLabel);

				oILGenerator.Emit(OpCodes.Ret);

				//

				FadeMulDYN = (FadeDelegate)oDynamicMethod.CreateDelegate(typeof(FadeDelegate));
			}

			#endregion

			//########################################
			//########################################

			#region MemoryCopy

			{
				var oDynamicMethod = new DynamicMethod
				(
					"MemoryCopy",
					typeof(void),
					new[] { typeof(IntPtr), typeof(IntPtr), typeof(int) },
					typeof(ManagedMemoryTools)
				);

				ILGenerator oILGenerator = oDynamicMethod.GetILGenerator();

				oILGenerator.Emit(OpCodes.Ldarg_0);
				oILGenerator.Emit(OpCodes.Ldarg_1);
				oILGenerator.Emit(OpCodes.Ldarg_2);
				oILGenerator.Emit(OpCodes.Cpblk);
				oILGenerator.Emit(OpCodes.Ret);

				MemoryCopy = (MemoryCopyDelegate)oDynamicMethod.CreateDelegate(typeof(MemoryCopyDelegate));
			}

			#endregion

			//########################################
			//########################################

			#region MemorySet

			{
				var oDynamicMethod = new DynamicMethod
				(
					"MemorySet",
					typeof(void),
					new[] { typeof(IntPtr), typeof(int), typeof(int) },
					typeof(ManagedMemoryTools)
				);

				ILGenerator oILGenerator = oDynamicMethod.GetILGenerator();

				oILGenerator.Emit(OpCodes.Ldarg_0);
				oILGenerator.Emit(OpCodes.Ldarg_1);
				oILGenerator.Emit(OpCodes.Ldarg_2);//nFadeDelta
				oILGenerator.Emit(OpCodes.Initblk);
				oILGenerator.Emit(OpCodes.Ret);

				MemorySet = (MemorySetDelegate)oDynamicMethod.CreateDelegate(typeof(MemorySetDelegate));
			}

			#endregion

			//########################################
			//########################################

			#region MemSetInt32

			{
				var oDynamicMethod = new DynamicMethod
				(
					"MemSetInt32",
					typeof(void),
					new[] { typeof(IntPtr), typeof(int) },
					typeof(ManagedMemoryTools)
				);

				ILGenerator oILGenerator = oDynamicMethod.GetILGenerator();

				oILGenerator.Emit(OpCodes.Ldarg_0);

				oILGenerator.Emit(OpCodes.Ldarg_1);
				oILGenerator.Emit(OpCodes.Stind_I4);

				oILGenerator.Emit(OpCodes.Ret);

				MemSetInt32 = (MemorySetInt32Delegate)oDynamicMethod.CreateDelegate(typeof(MemorySetInt32Delegate));
			}

			#endregion

			//########################################
			//########################################

			#region MemorySet32Dyn

			{
				var oDynamicMethod = new DynamicMethod
				(
					"MemorySet32Dyn",
					typeof(void),
					new[] { typeof(IntPtr), typeof(int), typeof(int) },
					typeof(ManagedMemoryTools)
				);

				ILGenerator oILGenerator = oDynamicMethod.GetILGenerator();

				Label oLoopLabel = oILGenerator.DefineLabel();
				Label oWhileLabel = oILGenerator.DefineLabel();

				LocalBuilder pDst = oILGenerator.DeclareLocal(typeof(int*));
				LocalBuilder nHalf = oILGenerator.DeclareLocal(typeof(int));
				LocalBuilder nCopyLength = oILGenerator.DeclareLocal(typeof(int));

				{
					//int* pDst = (int*)oDst.ToPointer();
					oILGenerator.Emit(OpCodes.Ldarga_S, (byte)0);
					oILGenerator.Emit(OpCodes.Call, typeof(IntPtr).GetMethod("ToPointer")!);
					oILGenerator.Emit(OpCodes.Stloc, pDst);

					//int nHalf = nCount >> 1;
					oILGenerator.Emit(OpCodes.Ldarg_2);//nFadeDelta
					oILGenerator.Emit(OpCodes.Ldc_I4_1);
					oILGenerator.Emit(OpCodes.Shr);
					oILGenerator.Emit(OpCodes.Stloc, nHalf);

					//int nCopyLength = 1;
					oILGenerator.Emit(OpCodes.Ldc_I4_1);
					oILGenerator.Emit(OpCodes.Stloc, nCopyLength);

					//*pDst = pValue;
					oILGenerator.Emit(OpCodes.Ldloc, pDst);
					oILGenerator.Emit(OpCodes.Ldarg_1);
					oILGenerator.Emit(OpCodes.Stind_I4);

					oILGenerator.Emit(OpCodes.Br_S, oWhileLabel);

					//oLoopLabel
					oILGenerator.MarkLabel(oLoopLabel);

					//pDst + nCopyLength
					oILGenerator.Emit(OpCodes.Ldloc, pDst);
					oILGenerator.Emit(OpCodes.Ldloc, nCopyLength);
					//////oILGenerator.Emit(OpCodes.Conv_I);
					oILGenerator.Emit(OpCodes.Ldc_I4_4);
					oILGenerator.Emit(OpCodes.Mul);
					oILGenerator.Emit(OpCodes.Add);
					//result on stack

					//pDst
					oILGenerator.Emit(OpCodes.Ldloc, pDst);
					//result on stack

					//nCopyLength * 4
					oILGenerator.Emit(OpCodes.Ldloc, nCopyLength);
					//oILGenerator.Emit(OpCodes.Ldc_I4_4);
					//oILGenerator.Emit(OpCodes.Mul);
					oILGenerator.Emit(OpCodes.Ldc_I4_2);
					oILGenerator.Emit(OpCodes.Shl);
					//result on stack

					//memcpy
					oILGenerator.Emit(OpCodes.Cpblk);
					//oILGenerator.Emit(OpCodes.Call, typeof(UnmanagedMemoryTools).GetMethod("MemCpy32"));

					//nCopyLength <<= 1;
					oILGenerator.Emit(OpCodes.Ldloc, nCopyLength);
					oILGenerator.Emit(OpCodes.Ldc_I4_1);
					oILGenerator.Emit(OpCodes.Shl);
					oILGenerator.Emit(OpCodes.Stloc, nCopyLength);

					//oWhileLabel:
					oILGenerator.MarkLabel(oWhileLabel);

					//while (nCopyLength < nHalf)
					oILGenerator.Emit(OpCodes.Ldloc, nCopyLength);
					oILGenerator.Emit(OpCodes.Ldloc, nHalf);
					oILGenerator.Emit(OpCodes.Blt_S, oLoopLabel);

					//pDst + nCopyLength
					oILGenerator.Emit(OpCodes.Ldloc, pDst);
					oILGenerator.Emit(OpCodes.Ldloc, nCopyLength);
					//////oILGenerator.Emit(OpCodes.Conv_I);
					oILGenerator.Emit(OpCodes.Ldc_I4_4);
					oILGenerator.Emit(OpCodes.Mul);
					oILGenerator.Emit(OpCodes.Add);
					//result on stack

					//pDst
					oILGenerator.Emit(OpCodes.Ldloc, pDst);
					//result on stack

					//(nCount - nCopyLength) * 4
					oILGenerator.Emit(OpCodes.Ldarg_2);//nFadeDelta
					oILGenerator.Emit(OpCodes.Ldloc, nCopyLength);
					oILGenerator.Emit(OpCodes.Sub);
					//oILGenerator.Emit(OpCodes.Ldc_I4_4);
					//oILGenerator.Emit(OpCodes.Mul);
					oILGenerator.Emit(OpCodes.Ldc_I4_2);
					oILGenerator.Emit(OpCodes.Shl);
					//result on stack

					//memcpy
					oILGenerator.Emit(OpCodes.Cpblk);

					//return
					oILGenerator.Emit(OpCodes.Ret);
				}

				MemorySet32Dyn = (MemorySetDelegate)oDynamicMethod.CreateDelegate(typeof(MemorySetDelegate));
			}

			#endregion

			//########################################
			//########################################
		}

		//################################################################################
		//################################################################################

		public static void FadeMul(IntPtr oTarget, int nCount, int nFadeDelta /*float nMultiplier*/)
		{
			//var nMultiplier = //var nMultiplier = 1.0F - ((float)m_nFadeDelta / (float)0xFF);

			unsafe
			{
				int* pTarget = (int*)oTarget;
				byte* pValue = (byte*)(pTarget);
				byte* pEnd = (byte*)(pTarget + nCount);

				while (pValue < pEnd)
				{
					//x = x - x * m_nFadeDelta / 0xFF;

					//pValue[0] -= (byte)(pValue[0] * nFadeDelta / 0xFF);
					//pValue[1] -= (byte)(pValue[1] * nFadeDelta / 0xFF);
					//pValue[2] -= (byte)(pValue[2] * nFadeDelta / 0xFF);

					if (pValue[0] > 0)
					{
						pValue[0] -= (byte)nFadeDelta;
					}

					if (pValue[1] > 0)
					{
						pValue[1] -= (byte)nFadeDelta;
					}

					if (pValue[2] > 0)
					{
						pValue[2] -= (byte)nFadeDelta;
					}

					//pValue[0] = (byte)(pValue[0] * nMultiplier);
					//pValue[1] = (byte)(pValue[1] * nMultiplier);
					//pValue[2] = (byte)(pValue[2] * nMultiplier);

					pValue += 4;
				}
			}
		}

		//################################################################################
		//################################################################################

		public static void FadeMulParallel(IntPtr oTarget, int nCount, int nFadeDelta /*float nMultiplier*/)
		{
			//var nMultiplier = //var nMultiplier = 1.0F - ((float)m_nFadeDelta / (float)0xFF);

			unsafe
			{
				int* pTarget = (int*)oTarget;
				//byte* pValue = (byte*)(pTarget);
				//byte* pEnd = (byte*)(pTarget + nCount);

				var oAction = new Action<int>(x =>
				{

					//while (pValue < pEnd)
					//{
					//x = x - x * m_nFadeDelta / 0xFF;

					//pValue[0] -= (byte)(pValue[0] * nFadeDelta / 0xFF);
					//pValue[1] -= (byte)(pValue[1] * nFadeDelta / 0xFF);
					//pValue[2] -= (byte)(pValue[2] * nFadeDelta / 0xFF);

					//int* pTarget = (int*)(oTarget + 4 * x);
					byte* pValue = (byte*)(pTarget + x);


					//pValue += 4;

					if (pValue[0] > 0)
					{
						pValue[0] -= (byte)nFadeDelta;
					}

					if (pValue[1] > 0)
					{
						pValue[1] -= (byte)nFadeDelta;
					}

					if (pValue[2] > 0)
					{
						pValue[2] -= (byte)nFadeDelta;
					}

					//pValue[0] = (byte)(pValue[0] * nMultiplier);
					//pValue[1] = (byte)(pValue[1] * nMultiplier);
					//pValue[2] = (byte)(pValue[2] * nMultiplier);

					//pValue += 4;
					//}

				});



				Parallel.For(0, nCount, oAction);

			}
		}

		//################################################################################
		//################################################################################
	}
}