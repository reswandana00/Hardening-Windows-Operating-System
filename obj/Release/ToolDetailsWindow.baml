   M S B A M L   `   `   `  ����   
Harder-WPF"    Harder_WPF.ToolDetailsWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     c _MahApps.Metro.IconPacks.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137ag cMahApps.Metro.IconPacks.Material, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137a:icon-http://metro.mahapps.com/winfx/xaml/iconpacks     � Title$"  Security Tools Configuration��$	��500��$	��450�� � AllowsTransparency .  � WindowStyle$
 None=� � 
ResizeMode$ NoResize=�
������  � WindowStartupLocation$ CenterOwner=� � 	Resources    ToolToggleStyle �� 
TargetType �� Property �� Value	 �� 
TargetType
 � ColumnDefinitions ��Name �� CornerRadius A�Name J� Effect3  -System.Windows.Media.Effects.DropShadowEffect   
BlurRadius   Opacity �� Triggers P� Property P� Value N� EnterActions �� 
TargetName �� TargetProperty	 t� To g� Duration	 �� To N� ExitActions  ActionButton �� Triggers��%}  &	        &	 %    �� " ��.���  � �� �� "	 ��.R�� 
 �� $��40 ��� $��* �.+��� $ SwitchBorder��$��0��$ 10�����#CCC$��20��$��36��$
��Left=�$��Center=�._�A� $ SwitchThumb��
�������$��16��$��16��$
��Left=�$��Center=�$��2,0,0,0q�   $ 2Q�$	 0.3Q��� $��1��#��z��$��Center=�$��10,0,0,0q�$��11(�$��Normal"����	#FF2C3E50$
��Wrap=� P�  �@d�	IsChecked$
 TrueY� �� .���� .��t� $ SwitchThumb��$ Margin�$ 18,0,0,0X�$ 0:0:0.2D��� $ SwitchBorder��$1 +(Border.Background).(SolidColorBrush.Color)�$ 	#FF4A90E2X�$ 0:0:0.2D� �� .���� .��t� $ SwitchThumb��$ Margin�$ 2,0,0,0X�$ 0:0:0.2D��� $ SwitchBorder��$1 +(Border.Background).(SolidColorBrush.Color)�$
 #CCCX�$ 0:0:0.2D��� " ��.���  � �� �	#FF4A90E2��  � ��
 �������  � ��$ 0q���  � ��$
 15,8q���  � ��$ SemiBold"���  �@�Cursor$
 Hand����  � �� �� "	 ��.R��� #��z��$ 5��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver . . ���  � �� �	#FF3A7BC8.���� $ 10�����#F8F9FA���	#FF4A90E2$��2q� �� Padding$ 20q�   $ 15Q�$	 0.3Q�._��  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0�� �� Orientation$ 
Horizontal=�$��0,0,0,15q�.+�.  (MahApps.Metro.IconPacks.PackIconMaterial     Kind$ Settings=�$��24��$��24�����	#FF4A90E2$��Center=��� -     ��Name$  
HeaderText��$"��Security Tools Configuration��$��18(�$
��Bold"����	#FF2C3E50$��Center=�$��10,0,0,0q��� $��1��$D��>Enable or disable individual security tools for this category:��$��12(����	#FF7F8C8D$��0,0,0,15q��� $��2��$
��Auto=�$��Disabled=�.���� -   ! ��Name$! 
ToolsPanel��$��0,0,0,10q��� $��3��$ 
Horizontal=�$��Right=�$��0,15,0,0q�.+��� -   $��
Select All��#��[ $��0,0,10,0q��� -   $��Deselect All��#��[ $��0,0,10,0q��� -   $��OK��#��[ $��0,0,10,0q��� -   $��Cancel��#��[ ���	#FF95A5A6