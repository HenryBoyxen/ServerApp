   M S B A M L   `   `   `  ????E  AExampleGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ExampleGUI
ExampleGUI  5          ExampleGUI.Server   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      *localclr-namespace:ExampleGUI     5        N? Title$  Server??5	      .??5      ?  ? RowDefinitions 5   
   ?? 5      $	??100 ?6   ?? 5      $??20 ?6   ?? 5      $	??100 ?6   ?? 5      .+?5   
   ?? $??Server??6   $??Center=?6"   $??Center=?6?   
???????$??50(?6m   $??0??6{   ? 5   
   $??2??6    ? ColumnDefinitions 5      ?? 5      ?? 5      $	??200 ?6#   ?? 5      $??50 ?6#   ?? 5      $	??200 ?6#   ?? 5      .+?5      ?? -   5         ??Name$ btn_server_start??5      $??Server starten??6N   $??1??6g   $	??100??6w   $	??200??6?   $??22(?6?   ???	#FF007EF9$??0q?6?   ???	#FF181E366?   ?? 5      -   5        $ btn_server_stop??5      {?.  $??Server stoppen??6^   $??3??6w   $	??100??6?   $	??200??6?   $??22(?6?   ???	#FF007EF9$??0q?6?   ???	#FF181E366?   5      ?? 5   
   -   5         ??Name$ 
txt_server??5      $??3??6(   
???????
???   ? ?? Padding$ 15q?6[   $??20,20,20,20q?6h   $
??Wrap=?6}   5      5       