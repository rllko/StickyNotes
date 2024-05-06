﻿using DapperGenericRepository.Repository;
using StickyNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StickyNotes
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        readonly int NoteId;
        public EditorWindow()
        {
            InitializeComponent();
        }

        public EditorWindow(string Text, int NoteId)
        {
            InitializeComponent();
            this.NoteId = NoteId;
            RichTextBox box = this.Content;
            box.Document.Blocks.Clear();
            box.Document.Blocks.Add(new Paragraph(new Run(Text)));
        }

        private void Window_Main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }


}
