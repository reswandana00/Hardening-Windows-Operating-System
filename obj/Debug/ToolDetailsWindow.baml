   M S B A M L   `   `   `  ����  
Harder-WPF"    Harder_WPF.ToolDetailsWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   c _MahApps.Metro.IconPacks.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137ag cMahApps.Metro.IconPacks.Material, Version=5.0.0.0, Culture=neutral, PublicKeyToken=0c0d510f9915137a:icon-http://metro.mahapps.com/winfx/xaml/iconpacks   5   	     � Title$"  Security Tools Configuration��5   	   $	��500��5   	   $	��450��6    � AllowsTransparency .  � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5	   	   
������  � WindowStartupLocation$ CenterOwner=�5   	    � 	Resources 5         ToolToggleStyle �� 
TargetType �� Property �� Value	 �� 
TargetType
 � ColumnDefinitions ��Name �� CornerRadius A�Name J� Effect3  -System.Windows.Media.Effects.DropShadowEffect   
BlurRadius   Opacity �� Triggers P� Property P� Value N� EnterActions �� 
TargetName �� TargetProperty	 t� To g� Duration	 �� To N� ExitActions  ActionButton �� Triggers��%  &	        &	 �    �� 5   
   " ��6(   .�5      ��  � �� 5      �� 5      "	 ��6&   .R�5      � 
 5      �� 5   "   $��40 �63   �� 5   "   $��* �63   .+�5      �� $ SwitchBorder��65   $��0��6%   $ 10��5   %   ���#CCC$��20��5   %   $��36��5   %   $
��Left=�5   %   $��Center=�5   %   ._�5    "   A� $ SwitchThumb��6*   
�������$��16��5"   *   $��16��5#   *   $
��Left=�5$   *   $��Center=�5%   *   $��2,0,0,0q�5&   *    5'   &     5(   *   $ 2Q�6;   $	 0.3Q�6J   5*   #   5+      �� 5.      $��1��6(   #��z��5/   (   $��Center=�50   (   $��10,0,0,0q�51   (   $��11(�52   (   $��Normal"�53   (   ���	#FF2C3E50$
��Wrap=�55   (   56       58      P� 59       �@d�	IsChecked$
 TrueY�6;    5:   "   �� 5;   &   .��5<   *   �� .��5=   .   t� $ SwitchThumb��6A   $ Margin�5>   A   $ 18,0,0,0X�5?   A   $ 0:0:0.2D�5@   A   �� 5A   .   $ SwitchBorder��6=   $1 +(Border.Background).(SolidColorBrush.Color)�5B   =   $ 	#FF4A90E2X�5C   =   $ 0:0:0.2D�5D   =   5E   +   5F   '    5H   "   �� 5I   &   .��5J   *   �� .��5K   .   t� $ SwitchThumb��6A   $ Margin�5L   A   $ 2,0,0,0X�5M   A   $ 0:0:0.2D�5N   A   �� 5O   .   $ SwitchBorder��6=   $1 +(Border.Background).(SolidColorBrush.Color)�5P   =   $
 #CCCX�5Q   =   $ 0:0:0.2D�5R   =   5S   +   5T   '   5V      5X      5Z      5[      �� 5]   
   " ��6%   .�5^      ��  � �� �	#FF4A90E26+   �� 5_       � ��
 �����6+   �� 5`       � ��$ 0q�60   �� 5a       � ��$
 15,8q�6(   �� 5b       � ��$ SemiBold"�6+   �� 5c       �@�Cursor$
 Hand��6'   �� 5d       � �� 5e      �� 5f      "	 ��6&   .R�5g      �� #��z��6!   $ 5��5h   !   ._�5i      �� $��Center=�6/   $��Center=�5j   /   5k      5l      5n       5o      P� 5p       �@J�IsMouseOver . . �5q      ��  � �� �	#FF3A7BC863   5r      5t      5u      .��5w      �� $ 10��6   ���#F8F9FA���	#FF4A90E2$��2q�5z       �� Padding$ 20q�5{       5|   
     5}      $ 15Q�6   $	 0.3Q�6/   ._�5�   
   �  � RowDefinitions 5�      �� 5�      $
��Auto �6    �� 5�      $
��Auto �6    �� 5�      $��* �6    �� 5�      $
��Auto �6    .+�5�      �� $��0��6    �� Orientation$ 
Horizontal=�6&   $��0,0,0,15q�6?   .+�5�      .  (MahApps.Metro.IconPacks.PackIconMaterial     Kind$ Settings=�6(   $��24��5�   (   $��24��5�   (   ���	#FF4A90E2$��Center=�5�   (   �� 5�      -   5          ��Name$  
HeaderText��5�      $"��Security Tools Configuration��5�      $��18(�5�      $
��Bold"�5�      ���	#FF2C3E50$��Center=�5�      $��10,0,0,0q�5�      5�      �� 5�      $��1��6   $D��>Enable or disable individual security tools for this category:��5�      $��12(�5�      ���	#FF7F8C8D$��0,0,0,15q�5�      �� 5�      $��2��6   $
��Auto=�5�      $��Disabled=�5�      .��5�      �� -   5        ! ��Name$! 
ToolsPanel��5�      $��0,0,0,10q�61   5�      �� 5�      $��3��6   $ 
Horizontal=�5�      $��Right=�5�      $��0,15,0,0q�5�      .+�5�      �� -   5        $��
Select All��5�      #��[ 5�      $��0,0,10,0q�5�      5�      �� 5�      -   5        $��Deselect All��5�      #��[ 5�      $��0,0,10,0q�5�      5�      �� 5�      -   5        $��OK��5�      #��[ 5�      $��0,0,10,0q�5�      5�      �� 5�      -   5        $��Cancel��5�      #��[ 5�      ���	#FF95A5A65�      5�      5�      5�      5�      