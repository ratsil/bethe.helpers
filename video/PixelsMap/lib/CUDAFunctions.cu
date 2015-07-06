#ifndef _INC_ARRAY_H_
#define _INC_ARRAY_H_
//"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v4.2\bin\nvcc.exe"  --cl-version 2010 --cubin %1CUDAFunctions.cu -ccbin "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\bin" -I "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\include" -m64 -arch=sm_21 -o "%1Properties\Resources\CUDAFunctions.21.x64.cubin"

#define byte unsigned char
#define ushort unsigned short

struct LayerInfo
{
	int nWidthDiff;
	int nForegroundStart;
	int nBackgroundStart;
	int nBackgroundStop;
	int nCropLeft;
	int nCropRight;
	int nWidth;
	int nAlphaConstant;
	int nCropWidth;
	int nCropHeight;
	float nShiftPosition;
	int bShiftVertical;
    int nAlphaType;
	int nOffsetLeft; 
	int nOffsetTop;
	float nShiftTotal;
};
struct MergeInfo
{
	int nLayersQty;
	int nBackgroundSize;
	int nBackgroundWidth;
	int nBackgroundAlphaType;
	LayerInfo aLayerInfos[];
};

extern "C" __global__ void CUDAFrameMerge(byte **pLayers, MergeInfo *pMergeInfo, byte *pAlphaMap)
{
	int nBGIndxPixel = blockIdx.x * blockDim.x + threadIdx.x; //это п. номер только по одному цвету и по BG !!

	if (nBGIndxPixel < pMergeInfo->nBackgroundSize) //2-й - это размер BG, 3-й - ширина BG, 4-й - делать ли задник? 5-й - инфа про FG1; 
	{									//Периодичность PRECOMPUTED_INFO_PERIOD - 1-й. 0-й - это количество слоёв
		int M, nIndxIndent, nRow;
		int nBGIndxRed, nBGIndxGreen, nBGIndxBlue, nBGIndxAlpha, nFGIndx;
		byte nFGColorRed, nFGColorGreen, nFGColorBlue, nFGColorAlpha;
		int nNextIndxRed, nNextIndxGreen, nNextIndxBlue, nNextIndxAlpha, nPixelAlphaIndx;	
		byte nPixelAlpha;
        int nMaskIndx = -1;
		LayerInfo* pLayerInfo;

		M = nBGIndxPixel / pMergeInfo->nBackgroundWidth; //M=(int)(BI/BW) т.е. с отбрасыванием дробной части.
		nIndxIndent = nBGIndxPixel - M * pMergeInfo->nBackgroundWidth;

		nBGIndxRed = nBGIndxPixel * 4;
		nBGIndxGreen = nBGIndxRed + 1;
		nBGIndxBlue = nBGIndxRed + 2;
		nBGIndxAlpha = nBGIndxRed + 3;
		pLayers[0][nBGIndxRed] = 0;
		pLayers[0][nBGIndxGreen] = 0;
		pLayers[0][nBGIndxBlue] = 0;
        if(1 == pMergeInfo->nBackgroundAlphaType)
            nMaskIndx = nBGIndxAlpha;
        else
            pLayers[0][nBGIndxAlpha] = pMergeInfo->nBackgroundAlphaType;

		for (ushort nLayerIndx = 1; pMergeInfo->nLayersQty > nLayerIndx; nLayerIndx++)
		{ 
			pLayerInfo = &pMergeInfo->aLayerInfos[(int)(nLayerIndx - 1)];
			if ((nBGIndxPixel >= pLayerInfo->nBackgroundStart) && (nBGIndxPixel <= pLayerInfo->nBackgroundStop) && (nIndxIndent >= pLayerInfo->nCropLeft) && (nIndxIndent <= pLayerInfo->nCropRight))
			{
				nFGIndx = (nBGIndxPixel + M * pLayerInfo->nWidthDiff - pLayerInfo->nForegroundStart) * 4;  
																								//формулу см. в методе Intersect.
                if (1 == pLayerInfo->nAlphaType) //леер является маской
                {
                    nMaskIndx = nFGIndx + 3;
                    continue;
                }
				nFGColorAlpha = pLayers[nLayerIndx][nFGIndx + 3];
                if (-1 < nMaskIndx) //применяем маску
                {
                    if (255 == pLayers[nLayerIndx - 1][nMaskIndx]) //отрезали пиксел по маске
                    {
                        nMaskIndx = -1;
                        continue;
                    }
                    nFGColorAlpha = (byte)(255.5 - (float)nFGColorAlpha * pLayers[nLayerIndx - 1][nMaskIndx] / 255);
                    nMaskIndx = -1;
                }
				nFGColorRed = pLayers[nLayerIndx][nFGIndx];
				nFGColorGreen = pLayers[nLayerIndx][nFGIndx + 1];
				nFGColorBlue = pLayers[nLayerIndx][nFGIndx + 2];
				
                if (0 == pLayerInfo->nAlphaType)
                {
					if (0 != pLayerInfo->nShiftPosition || 0 != pLayerInfo->nShiftTotal)  // && 1 > pLayerInfo->nShiftPosition && -1 < pLayerInfo->nShiftPosition
					{
						if (pLayerInfo->bShiftVertical)
						{
							if (0 < pLayerInfo->nShiftPosition)
							{
								nPixelAlpha = nFGColorAlpha;
								nFGColorAlpha = (byte)((nFGColorAlpha + 1) * (1 - pLayerInfo->nShiftPosition));
								nRow = M - (pLayerInfo->nBackgroundStart / pMergeInfo->nBackgroundWidth);
								if (nRow < (pLayerInfo->nCropHeight - 1))
								{
									nNextIndxRed = nFGIndx + (pLayerInfo->nWidth * 4);
									nNextIndxGreen = nNextIndxRed + 1;
									nNextIndxBlue = nNextIndxRed + 2;
									nNextIndxAlpha = nNextIndxRed + 3;
									if (0 < pLayers[nLayerIndx][nNextIndxAlpha])
									{
										if (0 < (nPixelAlpha = (byte)((pLayers[nLayerIndx][nNextIndxAlpha] + 1) * pLayerInfo->nShiftPosition)))
										{
											if (0 == nFGColorAlpha || 254 < nPixelAlpha)
											{
												nFGColorRed = pLayers[nLayerIndx][nNextIndxRed];
												nFGColorGreen = pLayers[nLayerIndx][nNextIndxGreen];
												nFGColorBlue = pLayers[nLayerIndx][nNextIndxBlue];
											}
											else
											{
												nPixelAlphaIndx = (nPixelAlpha - 1) * 65536;
												nFGColorRed = pAlphaMap[nPixelAlphaIndx + (256 * nFGColorRed) + pLayers[nLayerIndx][nNextIndxRed]];
												nFGColorGreen = pAlphaMap[nPixelAlphaIndx + (256 * nFGColorGreen) + pLayers[nLayerIndx][nNextIndxGreen]];
												nFGColorBlue = pAlphaMap[nPixelAlphaIndx + (256 * nFGColorBlue) + pLayers[nLayerIndx][nNextIndxBlue]];
											}
										}
										if (255 < nFGColorAlpha + nPixelAlpha)
											nFGColorAlpha = 255;
										else
											nFGColorAlpha += nPixelAlpha;
									}
								}
							}
							else
							{
							}
						}
						else
						{
							if (0 < pLayerInfo->nShiftPosition)
							{
							
							}
							else
							{
							}
						}
					}
				
					nPixelAlpha = pLayerInfo->nAlphaConstant;

					if (255 == nPixelAlpha)
						nPixelAlpha = nFGColorAlpha;
					else if (0 == nFGColorAlpha)
						nPixelAlpha=0;
					else if (0 < nPixelAlpha && 255 > nFGColorAlpha)                        // объединение альфы слоя с константной альфой !!!!
						nPixelAlpha = (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);
				}
                else
                    nPixelAlpha = 255;
					
				if (0 < nPixelAlpha)
				{
					if (255 == nPixelAlpha || 0 == pLayers[0][nBGIndxAlpha])
					{
						pLayers[0][nBGIndxRed] = nFGColorRed;
						pLayers[0][nBGIndxGreen] = nFGColorGreen;
						pLayers[0][nBGIndxBlue] = nFGColorBlue;
					}
					else
					{							//индекс меньше, т.к. 0-е значение альфы мы не считаем и все индексы сдвинулись...
						nPixelAlphaIndx = (nPixelAlpha - 1) * 65536;
						pLayers[0][nBGIndxRed] = pAlphaMap[nPixelAlphaIndx + (256 * pLayers[0][nBGIndxRed]) + nFGColorRed];
						pLayers[0][nBGIndxGreen] = pAlphaMap[nPixelAlphaIndx + (256 * pLayers[0][nBGIndxGreen]) + nFGColorGreen];
						pLayers[0][nBGIndxBlue] = pAlphaMap[nPixelAlphaIndx + (256 * pLayers[0][nBGIndxBlue]) + nFGColorBlue];
					}
					if (pLayers[0][nBGIndxAlpha] < nPixelAlpha)   // очередная попытка примирить альфу с действительностью ))
						pLayers[0][nBGIndxAlpha] = nPixelAlpha;
				}
			}
		}
	}
}
#endif // _INC_ARRAY_H_
