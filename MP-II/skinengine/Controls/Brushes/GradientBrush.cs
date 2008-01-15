#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MediaPortal.Core;
using MediaPortal.Core.Properties;
using SlimDX;
using SlimDX.Direct3D;
using SlimDX.Direct3D9;
using SkinEngine.DirectX;
namespace SkinEngine.Controls.Brushes
{
  

  public enum BrushMappingMode
  {
    Absolute,
    RelativeToBoundingBox
  };

  public enum ColorInterpolationMode
  {
    ColorInterpolationModeScRgbLinearInterpolation,
    ColorInterpolationModeSRgbLinearInterpolation
  };

  public enum GradientSpreadMethod
  {
    Pad,
    Reflect,
    Repeat
  };


  public class GradientBrush : Brush, IList
  {
    protected Texture _gradientTexture;
    protected PositionColored2Textured[] _verts;
    Property _colorInterpolationModeProperty;
    Property _gradientStopsProperty;
    Property _spreadMethodProperty;
    Property _mappingModeProperty;


    /// <summary>
    /// Initializes a new instance of the <see cref="GradientBrush"/> class.
    /// </summary>
    public GradientBrush()
    {
      Init();
    }

    public GradientBrush(GradientBrush b)
      : base(b)
    {
      Init();
      ColorInterpolationMode = b.ColorInterpolationMode;
      SpreadMethod = b.SpreadMethod;
      MappingMode = b.MappingMode;
      foreach (GradientStop stop in b.GradientStops)
      {
        GradientStop s = new GradientStop();
        s.Color = stop.Color;
        s.Offset = stop.Offset;
        GradientStops.Add(s);
      }
    }
    void Init()
    {
      _gradientStopsProperty = new Property(new GradientStopCollection(this));
      _colorInterpolationModeProperty = new Property(ColorInterpolationMode.ColorInterpolationModeScRgbLinearInterpolation);
      _spreadMethodProperty = new Property(GradientSpreadMethod.Pad);
      _mappingModeProperty = new Property(BrushMappingMode.RelativeToBoundingBox);

      _gradientStopsProperty.Attach(new PropertyChangedHandler(OnPropertyChanged));
      _colorInterpolationModeProperty.Attach(new PropertyChangedHandler(OnPropertyChanged));
      _spreadMethodProperty.Attach(new PropertyChangedHandler(OnPropertyChanged));
      _mappingModeProperty.Attach(new PropertyChangedHandler(OnPropertyChanged));
    }

    /// <summary>
    /// Called when one of the gradients changed.
    /// </summary>
    public void OnGradientsChanged()
    {
      OnPropertyChanged(GradientStopsProperty);
    }

    /// <summary>
    /// Gets or sets the color interpolation mode property.
    /// </summary>
    /// <value>The color interpolation mode property.</value>
    public Property ColorInterpolationModeProperty
    {
      get
      {
        return _colorInterpolationModeProperty;
      }
      set
      {
        _colorInterpolationModeProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the color interpolation mode.
    /// </summary>
    /// <value>The color interpolation mode.</value>
    public ColorInterpolationMode ColorInterpolationMode
    {
      get
      {
        return (ColorInterpolationMode)_colorInterpolationModeProperty.GetValue();
      }
      set
      {
        _colorInterpolationModeProperty.SetValue(value);
      }
    }

    /// <summary>
    /// Gets or sets the gradient stops property.
    /// </summary>
    /// <value>The gradient stops property.</value>
    public Property GradientStopsProperty
    {
      get
      {
        return _gradientStopsProperty;
      }
      set
      {
        _gradientStopsProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the gradient stops.
    /// </summary>
    /// <value>The gradient stops.</value>
    public GradientStopCollection GradientStops
    {
      get
      {
        return (GradientStopCollection)_gradientStopsProperty.GetValue();
      }
      set
      {
        _gradientStopsProperty.SetValue(value);
      }
    }

    /// <summary>
    /// Gets or sets the mapping mode property.
    /// </summary>
    /// <value>The mapping mode property.</value>
    public Property MappingModeProperty
    {
      get
      {
        return _mappingModeProperty;
      }
      set
      {
        _mappingModeProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the mapping mode.
    /// </summary>
    /// <value>The mapping mode.</value>
    public BrushMappingMode MappingMode
    {
      get
      {
        return (BrushMappingMode)_mappingModeProperty.GetValue();
      }
      set
      {
        _mappingModeProperty.SetValue(value);
      }
    }

    /// <summary>
    /// Gets or sets the spread method property.
    /// </summary>
    /// <value>The spread method property.</value>
    public Property SpreadMethodProperty
    {
      get
      {
        return _spreadMethodProperty;
      }
      set
      {
        _spreadMethodProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the spread method.
    /// </summary>
    /// <value>The spread method.</value>
    public GradientSpreadMethod SpreadMethod
    {
      get
      {
        return (GradientSpreadMethod)_spreadMethodProperty.GetValue();
      }
      set
      {
        _spreadMethodProperty.SetValue(value);
      }
    }

    /// <summary>
    /// Sets the color.
    /// </summary>
    /// <param name="vertexbuffer">The vertexbuffer.</param>
    protected void SetColor(VertexBuffer vertexbuffer)
    {
      ColorValue color = ColorConverter.FromColor(GradientStops[0].Color);
      color.Alpha *= (float)Opacity;
      for (int i = 0; i < _verts.Length; ++i)
      {
        _verts[i].Color = color.ToArgb();
      }

      PositionColored2Textured.Set(vertexbuffer, ref _verts);
    }

    protected void CreateGradient()
    {
      LockedRect rect = _gradientTexture.LockRectangle(0, LockFlags.None);
      //int[,] buffer = (int[,])_gradientTexture.LockRectangle(typeof(int), 0, LockFlags.None, new int[] { (int)2, (int)256 });
      float width = 256.0f;
      byte[] data = new byte[4 * 512];
      int offY = 256 * 4;
      for (int i = 0; i < GradientStops.Count - 1; ++i)
      {
        GradientStop stopbegin = GradientStops[i];
        GradientStop stopend = GradientStops[i + 1];
        ColorValue colorStart = ColorConverter.FromColor(stopbegin.Color);
        ColorValue colorEnd = ColorConverter.FromColor(stopend.Color);
        int offsetStart = (int)(stopbegin.Offset * width);
        int offsetEnd = (int)(stopend.Offset * width);

        float distance = offsetEnd - offsetStart;
        for (int x = offsetStart; x < offsetEnd; ++x)
        {
          float step = (x - offsetStart) / distance;
          float r = step * (colorEnd.Red - colorStart.Red);
          r += colorStart.Red;

          float g = step * (colorEnd.Green - colorStart.Green);
          g += colorStart.Green;

          float b = step * (colorEnd.Blue - colorStart.Blue);
          b += colorStart.Blue;

          float a = step * (colorEnd.Alpha - colorStart.Alpha);
          a += colorStart.Alpha;

          Color color;
          if (IsOpacityBrush)
            color = ColorConverter.FromColor(a, a, a, 1.0f);
          else
            color = ColorConverter.FromColor(a, r, g, b);

          int offx = x * 4;
          data[offx] = (byte)color.B;
          data[offx + 1] = (byte)color.G;
          data[offx + 2] = (byte)color.R;
          data[offx + 3] = (byte)color.A;

          data[offY + offx] = (byte)color.B;
          data[offY + offx + 1] = (byte)color.G;
          data[offY + offx + 2] = (byte)color.R;
          data[offY + offx + 3] = (byte)color.A;

        }
      }
      rect.Data.Write(data, 0, 4 * 512);
      _gradientTexture.UnlockRectangle(0);
      rect.Data.Dispose();

    }

    #region IList Members

    public int Add(object value)
    {
      GradientStops.Add((GradientStop)value);
      return GradientStops.Count;
    }

    public void Clear()
    {
      GradientStops.Clear();
    }

    public bool Contains(object value)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public int IndexOf(object value)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public void Insert(int index, object value)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public bool IsFixedSize
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public bool IsReadOnly
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public void Remove(object value)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public void RemoveAt(int index)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public object this[int index]
    {
      get
      {
        throw new Exception("The method or operation is not implemented.");
      }
      set
      {
        throw new Exception("The method or operation is not implemented.");
      }
    }

    #endregion

    #region ICollection Members

    public void CopyTo(Array array, int index)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public int Count
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public bool IsSynchronized
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public object SyncRoot
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    #endregion

    #region IEnumerable Members

    public IEnumerator GetEnumerator()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion
  }
}
