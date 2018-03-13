﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HandSchool.Models
{
    public class CurriculumLabel : Label
    {
        public CurriculumItem Context { get; }
        public Span Title = new Span { FontAttributes = FontAttributes.Bold, ForegroundColor = Color.White };
        public Span At = new Span { Text = " @ ", ForegroundColor = Color.FromRgba(255, 255, 255, 160) };
        public Span Where = new Span { ForegroundColor = Color.FromRgba(255, 255, 255, 220) };
        
        public CurriculumLabel(CurriculumItem value)
        {
            Context = value;
            HorizontalTextAlignment = TextAlignment.Center;
            VerticalTextAlignment = TextAlignment.Center;
            BindingContext = this;
            FormattedText = new FormattedString { Spans = { Title, At, Where } };
            Update();
        }

        public void Update()
        {
            Grid.SetColumn(this, Context.WeekDay);
            Grid.SetRow(this, Context.DayBegin);
            Grid.SetRowSpan(this, Context.DayEnd - Context.DayBegin + 1);
            Title.Text = Context.Name;
            Where.Text = Context.Classroom;
            BackgroundColor = GetColor();
        }

        public Color GetColor()
        {
            return Color.LimeGreen;
        }
    }
}