﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;

namespace elastic_collision_simulator
{
  interface ICollisionManager
  {
    Rectangle BoundingBox
    {
      get;
    }
  }

  class CollisionManager<T> where T : ICollisionManager
  {
    public Bitmap Boxes
    {
      get
      {
        Bitmap bitmap = new Bitmap(_screen.Width, _screen.Height);
        Graphics graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.White);

        for (int i = 0; i < _screen.Width; i += _boxSize)
        {
          graphics.DrawLine(new Pen(new SolidBrush(Color.Red)), i, 0, i, _screen.Height);
        }

        for (int i = 0; i < _screen.Height; i += _boxSize)
        {
          graphics.DrawLine(new Pen(new SolidBrush(Color.Red)), 0, i, _screen.Width, i);
        }

        return bitmap;
      }
    }

    public CollisionManager(Rectangle _screen, int _boxSize)
    {
      this._screen = _screen;
      this._boxSize = _boxSize;
      _cols = (int)Math.Ceiling((double)_screen.Width / _boxSize);
      _rows = (int)Math.Ceiling((double)_screen.Height / _boxSize);
    }

    public void Init()
    {
      _boxes = new Dictionary<int, List<T>>();
      _locks = new object[_cols * _rows];

      for (int i = 0; i < _cols * _rows; i++)
      {
        _boxes.Add(i, new List<T>());
        _locks[i] = new object();
      }
    }

    public void Add(T obj)
    {
      calculateIds(obj).ForEach(id =>
      {
        lock (_locks[id])
        {
          _boxes[id].Add(obj);
        }
      });
    }

    public List<T> Candidates(T obj)
    {
      List<T> candidates = new List<T>();

      calculateIds(obj).ForEach(id => _boxes[id].ForEach(candidate => addToList<T>(candidates, candidate)));
      candidates.Remove(obj);

      return candidates;
    }

    private readonly Rectangle _screen;
    private readonly int _boxSize, _cols, _rows;
    private Dictionary<int, List<T>> _boxes;
    private object[] _locks;

    private List<int> calculateIds(T obj)
    {
      Rectangle boundingBox = obj.BoundingBox;
      System.Drawing.Point topLeft = boundingBox.Location,
        bottomRight = boundingBox.Location + boundingBox.Size;
      List<int> ids = new List<int>();

      addToList<int>(ids, hash(topLeft));
      addToList<int>(ids, hash(new System.Drawing.Point(topLeft.X, bottomRight.Y)));
      addToList<int>(ids, hash(new System.Drawing.Point(bottomRight.X, topLeft.Y)));
      addToList<int>(ids, hash(bottomRight));

      return ids;
    }

    private int hash(System.Drawing.Point point)
    {
      return (int)(Math.Floor((double)point.X / _boxSize)
        + Math.Floor((double)point.Y / _boxSize) * _cols);
    }

    private void addToList<U>(List<U> list, U value)
    {
      if (!list.Contains(value))
      {
        list.Add(value);
      }
    }
  }
}
