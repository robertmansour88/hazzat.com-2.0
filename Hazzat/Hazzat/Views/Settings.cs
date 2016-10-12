using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace Hazzat.Views
{
    public class Settings : ContentPage
    {
        public Settings()
        {
            Button GoBack, Update;

            GoBack = new Button { HorizontalOptions = LayoutOptions.Start, Text = "Save" };

            GoBack.Clicked += Back;

            Content = new TableView
            {
                HasUnevenRows = true,

                Root = new TableRoot("Settings") {

                                new TableSection ("Font Size") {
                                    new ViewCell {View = new StackLayout {
                                      Children = {
                                            new Slider { Value = 0.5 },
                                            new Label { Text = "Adjusts the font size across the Hymns Pages" }
                                        } } }
                                },
                                new TableSection("Update Cache") {
                                    new ViewCell { View = new StackLayout { Children = { 
                                                    new Button { Text = "Update" },
                                                    new Label { Text = "Cache is used when there is no internet connection." }
                                        } } }
                                },

                                new TableSection("About") {
                                    new ViewCell { View = new Label { FormattedText = "version 1.0. © --" } }
                                } ,

                                new TableSection() {
                                     new ViewCell {View = GoBack }
                                 }
                               },
                Intent = TableIntent.Settings

            };
        }

        void Back(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Navigation.PopModalAsync();
            });
        }
    }
}
