; Source: https://nsis.sourceforge.io/StrReplace_v4
!define Var0 $R0
!define Var1 $R1
!define Var2 $R2
!define Var3 $R3
!define Var4 $R4
!define Var5 $R5
!define Var6 $R6
!define Var7 $R7
!define Var8 $R8
 
!macro StrReplaceV4 Var Replace With In
 Push `${Replace}`
 Push `${With}`
 Push `${In}`
  Call StrReplaceV4
 Pop `${Var}`
!macroend
!define StrReplaceV4 `!insertmacro StrReplaceV4`
 
Function StrReplaceV4
Exch ${Var0} #in
Exch 1
Exch ${Var1} #with
Exch 2
Exch ${Var2} #replace
Push ${Var3}
Push ${Var4}
Push ${Var5}
Push ${Var6}
Push ${Var7}
Push ${Var8}
 
 StrCpy ${Var3} -1
 StrLen ${Var5} ${Var0}
 StrLen ${Var6} ${Var1}
 StrLen ${Var7} ${Var2}
 Loop:
  IntOp ${Var3} ${Var3} + 1
  StrCpy ${Var4} ${Var0} ${Var7} ${Var3}
  StrCmp ${Var3} ${Var5} End
  StrCmp ${Var4} ${Var2} 0 Loop
 
   StrCpy ${Var4} ${Var0} ${Var3}
   IntOp ${Var8} ${Var3} + ${Var7}
   StrCpy ${Var8} ${Var0} "" ${Var8}
   StrCpy ${Var0} ${Var4}${Var1}${Var8}
   IntOp ${Var3} ${Var3} + ${Var6}
   IntOp ${Var3} ${Var3} - 1
   IntOp ${Var5} ${Var5} - ${Var7}
   IntOp ${Var5} ${Var5} + ${Var6}
 
 Goto Loop
 End:
 
Pop ${Var8}
Pop ${Var7}
Pop ${Var6}
Pop ${Var5}
Pop ${Var4}
Pop ${Var3}
Pop ${Var2}
Exch
Pop ${Var1}
Exch ${Var0} #out
FunctionEnd
 
!undef Var8
!undef Var7
!undef Var6
!undef Var5
!undef Var4
!undef Var3
!undef Var2
!undef Var1
!undef Var0