using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effct : MonoBehaviour
{
    [SerializeField]
    private Shader _shader;
    [SerializeField, Range(4, 16)]
    private int _sampleCount = 8;
    [SerializeField, Range(0.0f, 1.0f)]
    private float _strength = 0.5f;
    [SerializeField]
    public Texture _subTex;
    private Material _material;

    private PlayerController playerController;
    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }
    private void Update()
    {
        _strength = playerController.speed*0.01f;
    }
    private void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        if (_material == null)
        {
            if (_shader == null)
            {
                Graphics.Blit(source, dest);
                return;
            }
            else
            {
                _material = new Material(_shader);
            }
        }
        _material.SetInt("_SampleCount", _sampleCount);
        _material.SetFloat("_Strength", _strength);
        _material.SetTexture("_SubTex", _subTex);
        Graphics.Blit(source, dest, _material);
    }
}
