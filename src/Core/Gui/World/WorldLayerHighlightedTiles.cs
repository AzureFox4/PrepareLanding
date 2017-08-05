﻿using System.Collections;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PrepareLanding.Core.Gui.World
{
    public class WorldLayerHighlightedTiles : WorldLayer
    {
        private readonly Material _defaultMaterial = new Material(WorldMaterials.SelectedTile);

        private readonly List<Vector3> _vertices = new List<Vector3>();

        private float _alpha;
        protected override float Alpha => _alpha;

        private AlphaRampDirection _alphaRampDirection;

        private Color _materialColor = Color.green;

        private readonly float _blinkTick;

        public Color TileColor
        {
            get { return _materialColor; }
            set
            {
                _materialColor = value;
                _defaultMaterial.color = _materialColor;
            }
        }

        public WorldLayerHighlightedTiles()
        {
            _defaultMaterial.color = TileColor;
            var matColor = _defaultMaterial.color;
            matColor.a = 1.0f;
            _defaultMaterial.color = matColor;

            _blinkTick = PrepareLanding.Instance.TileHighlighter.BlinkTick;

            PrepareLanding.Instance.TileHighlighter.HighlightedTilesWorldLayer = this;

            PrepareLanding.Instance.GameTicks.TicksIntervalElapsed += OnTicksIntervalElapsed;
            PrepareLanding.Instance.GameTicks.UpdateInterval = _blinkTick;
        }

        public void SetAlpha(float alpha)
        {
            if (alpha > 1.0f)
                alpha = 1.0f;

            if (alpha < 0.0f)
                alpha = 0.0f;

            _alpha = alpha;
        }

        private void OnTicksIntervalElapsed()
        {
            if (PrepareLanding.Instance.TileHighlighter.DisableTileHighlighting)
            {
                _alpha = 0;
                return;
            }

            if (PrepareLanding.Instance.TileHighlighter.DisableTileBlinking)
                _alpha = TileHighlighter.DefaultTileHighlightingAlphaValue;
            else
                TileBreath();
        }

        public override IEnumerable Regenerate()
        {
            foreach (var result in base.Regenerate())
            {
                yield return result;
            }

            foreach (var tileId in PrepareLanding.Instance.TileHighlighter.HighlightedTilesIds)
            {
                if (tileId < 0)
                    continue;

                var subMesh = GetSubMesh(_defaultMaterial);
                Find.World.grid.GetTileVertices(tileId, _vertices);

                var startVertIndex = subMesh.verts.Count;
                var currentIndex = 0;
                var maxCount = _vertices.Count;

                while (currentIndex < maxCount)
                {
                    if (currentIndex % 1000 == 0)
                    {
                        yield return null;
                    }

                    if (subMesh.verts.Count > 60000)
                    {
                        subMesh = GetSubMesh(_defaultMaterial);
                    }

                    subMesh.verts.Add(_vertices[currentIndex] + _vertices[currentIndex].normalized * 0.012f);
                    if (currentIndex < maxCount - 2)
                    {
                        subMesh.tris.Add(startVertIndex + currentIndex + 2);
                        subMesh.tris.Add(startVertIndex + currentIndex + 1);
                        subMesh.tris.Add(startVertIndex);
                    }
                    currentIndex++;
                }

                FinalizeMesh(MeshParts.All);
            }
        }

        protected virtual void TileBreath()
        {
            if (_alpha < float.Epsilon)
            {
                _alpha = 0f;
                _alphaRampDirection = AlphaRampDirection.AlphaRampUp;
            }
            if (_alpha >= 1f)
            {
                _alpha = 1f;
                _alphaRampDirection = AlphaRampDirection.AlphaRampDown;
            }

            if (_alphaRampDirection == AlphaRampDirection.AlphaRampDown)
                _alpha -= _blinkTick;
            else
                _alpha += _blinkTick;
        }

        private enum AlphaRampDirection
        {
            AlphaRampUp,
            AlphaRampDown
        }
    }
}
