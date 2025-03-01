using Android.App;
using Android.Content.Res;
using Android.Runtime;

namespace PromTech.Mobile.App;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        //Хендлеры для тонкой подстройки контролов
        //PS зачем эти костыли ума не приложу
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
        {
            if (view is Entry)
            {
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);//Делаем линию у Entry невидимой
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                //handler.PlatformView.SetHintTextColor(ColorStateList.ValueOf(Android.Graphics.Color.Red));//Красим PlaceHolder в нужный цвет
            }
        });
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}