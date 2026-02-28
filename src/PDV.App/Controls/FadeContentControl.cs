using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PDV.App.Controls;

public class FadeContentControl : ContentControl
{
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (newContent == null) return;

        var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
        {
            EasingFunction = new QuadraticEase()
        };

        BeginAnimation(OpacityProperty, animation);
    }
}
