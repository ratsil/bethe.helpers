using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime;
using System.Threading;
using System.Runtime.InteropServices;

namespace helpers
{
    public partial class DisCom
    {
        static private void MergingCUDATest(int nBGIndxLine)
        {
            for (int nI = 0; nI < _cMergeInfo.nBackgroundWidth; nI++)
                MergingCUDATest(nBGIndxLine, nI);
        }
        static private void MergingCUDATest(int nBGIndxLine, int nBGIndxRed)
        {
            if (nBGIndxLine >= _cMergeInfo.nBackgroundHight || nBGIndxRed >= _cMergeInfo.nBackgroundWidth)
                return;

            int nBGIndxPixelStartRed = nBGIndxLine * _cMergeInfo.nBackgroundWidth_4;
            nBGIndxRed = nBGIndxPixelStartRed + 4 * nBGIndxRed;

            LayerInfo cLayerInfo;
            int nBGIndxGreen, nBGIndxBlue, nBGIndxAlpha, nFGIndxRed;
            byte nFGColorRed = 0, nFGColorGreen = 0, nFGColorBlue = 0, nFGColorAlpha = 0, nExtMaskAlpha; // =0 не нужно в куде, т.к. здесь ругается компилер
            int nPixelAlphaIndx;
            byte nPixelAlpha, nMaskAllUpper, nMaskCurrent;
            ushort nLayerIndx;
            bool bMainCondition, bFieldsCondition = false;
            bool bElse;     // нужна для экономии, чтоб не каждый раз присваивать, а только когда надо
            bool bBGChanged;        // тоже чтобы менять, только если не менялся фон
            bool bBGAChanged;        // тоже чтобы менять, только если не менялся фон
            int nRedPlusDelta;
            int nUpPxIndx;
            int nRowBeginingIndx;
            int nLeftPxIndx;
            int nRowEndingIndx;

            // ex cLLI  (struct LineLayerInfo)
            int nBGCropStartRed; // before main if     (+)
            int nFGCropStartRed = 0; // in main if (1 time)   (*)
            int nBGCropEndRed;  // before main if    (+ -)
            bool bRowUpper;  // for shift      (- -)
            int nBgFgLinesDelta;   // for fields (1 time)   (-)
            int nFGLineBeginningRed; // for fields and shift  (* /)
            // ---------

            if (255 == _cMergeInfo.nBackgroundAlphaType || 0 == _cMergeInfo.nBackgroundAlphaType)
            {
                _cDisComProcessing._aLayers[0][nBGIndxRed + 3] = _cMergeInfo.nBackgroundAlphaType;
                bBGAChanged = true;
            }
            else
                bBGAChanged = false;

            nMaskAllUpper = 0;
            nExtMaskAlpha = 0;
            bBGChanged = false;

            for (nLayerIndx = 1; _cMergeInfo.nLayersQty > nLayerIndx; nLayerIndx++)
            {
                cLayerInfo = _cMergeInfo.aLayerInfos[(int)(nLayerIndx - 1)];

                // ex cLLI init
                if (nBGIndxLine < cLayerInfo.nCropTopLineInBG || nBGIndxLine > cLayerInfo.nCropBottomLineInBG)
                {
                    nBGCropStartRed = int.MaxValue;   // not main_if
                    nFGCropStartRed = int.MinValue;
                    nBGCropEndRed = int.MinValue;
                }
                else
                {
                    nBGCropStartRed = nBGIndxPixelStartRed + cLayerInfo.nCropLeft_4;
                    nBGCropEndRed = nBGCropStartRed + cLayerInfo.nCropWidth_4 - 4;
                }
                // -----------

                bMainCondition = nBGIndxRed >= nBGCropStartRed && nBGIndxRed <= nBGCropEndRed;

                if (cLayerInfo.nAlphaType > 0 && cLayerInfo.nAlphaType < 5)  // т.е. маска типа 1,2,3,4
                {
                    if (nExtMaskAlpha < 255 || (cLayerInfo.nAlphaType == 3 || cLayerInfo.nAlphaType == 4) && nMaskAllUpper < 255)
                    {
                        if (bMainCondition)
                        {
                            // ex cLLI init
                            if (cLayerInfo.nCropTopLineInBG == 0)
                                nFGCropStartRed = (nBGIndxLine - cLayerInfo.nTop) * cLayerInfo.nWidth_4;
                            else
                                nFGCropStartRed = (nBGIndxLine - cLayerInfo.nCropTopLineInBG) * cLayerInfo.nWidth_4;
                            if (nBGIndxPixelStartRed >= nBGCropStartRed)
                                nFGCropStartRed -= cLayerInfo.nLeft_4;
                            // -----------

                            nMaskCurrent = _cDisComProcessing._aLayers[nLayerIndx][(nBGIndxRed - nBGCropStartRed + nFGCropStartRed) + 3];
                        }
                        else
                            nMaskCurrent = 0;    // т.е. маски нет тут.        // лаер является маской   и там, где слоя вообще нет, то всё-равно отрезаем, т.к. на то она и маска, что где картинки нет, то считаем это всё альфой

                        if (cLayerInfo.nAlphaType == 2 || cLayerInfo.nAlphaType == 4) // т.е. инвертированная маска
                            nMaskCurrent = (byte)(255 - nMaskCurrent);

                        if (cLayerInfo.nAlphaType < 3)
                            nExtMaskAlpha = (byte)(nExtMaskAlpha + nMaskCurrent > 255 ? 255 : nExtMaskAlpha + nMaskCurrent); // gcc понимает это так же, т.е. (nExtMaskAlpha + nMaskCurrent)

                        if (cLayerInfo.nAlphaType == 3 || cLayerInfo.nAlphaType == 4)  // т.е. all upper mask
                            nMaskAllUpper = (byte)(nMaskAllUpper + nMaskCurrent > 255 ? 255 : nMaskAllUpper + nMaskCurrent);
                    }
                    continue;
                }
                nExtMaskAlpha = (byte)(nExtMaskAlpha + nMaskAllUpper > 255 ? 255 : nExtMaskAlpha + nMaskAllUpper);

                if (255 == nExtMaskAlpha) //отрезали пиксел по маске, т.е. наш слой стал A = 0
                {
                    nExtMaskAlpha = 0;
                    continue;
                }

                bFieldsCondition = false;
                if (!bMainCondition)
                {
                    nRedPlusDelta = nBGIndxRed + cLayerInfo.nHalfDeltaPxX_4;
                    bFieldsCondition = cLayerInfo.nShiftTotalX != 0 &&    // есть поля в слое
                                        (cLayerInfo.nHalfDeltaPxX_4 > 0 && nRedPlusDelta >= nBGCropStartRed && nRedPlusDelta <= nBGCropEndRed ||    // движение вправо и можно показать поля за рамками кропа
                                        cLayerInfo.nHalfDeltaPxX_4 < 0 && nRedPlusDelta <= nBGCropEndRed && nRedPlusDelta >= nBGCropStartRed);    // движение влево и можно показать поля за рамками кропа
                }
                if (bMainCondition || bFieldsCondition)   //main_if
                {
                    // ex cLLI init
                    if (nBGCropStartRed != int.MaxValue)
                    {
                        if (cLayerInfo.nCropTopLineInBG == 0)
                            nFGCropStartRed = (nBGIndxLine - cLayerInfo.nTop) * cLayerInfo.nWidth_4;
                        else
                            nFGCropStartRed = (nBGIndxLine - cLayerInfo.nCropTopLineInBG) * cLayerInfo.nWidth_4;
                        if (nBGIndxPixelStartRed >= nBGCropStartRed)
                            nFGCropStartRed -= cLayerInfo.nLeft_4;
                    }
                    // -----------

                    nFGIndxRed = (nBGIndxRed - nBGCropStartRed + nFGCropStartRed);
                    bElse = false;

                    //if (0 == cLayerInfo.nAlphaType) // т.е. наш слой не альфирующий, а обычный слой с альфой RGBA     // вроде не доёдем же сюда с > 0 ????  надо попробовать убрать // попробовал ))
                    //{
                    #region обработка шифтов и движений
                    if (0 == cLayerInfo.nShiftTotalX) // поля не нужны и обрабатываем только неточное положение пикселя
                    {
                        if (0 != cLayerInfo.nShiftPositionByteX || 0 != cLayerInfo.nShiftPositionByteY)
                        {
                            if (0 != cLayerInfo.nShiftPositionByteX && 0 == cLayerInfo.nShiftPositionByteY)  // попали не точно в пиксель по Х   (берём с левого пикселя)  0 <= S < 255
                            {
                                #region не точно по X
                                nLeftPxIndx = nFGIndxRed - 4;
                                // ex cLLI init
                                if (nBGIndxPixelStartRed < nBGCropStartRed)
                                    nFGLineBeginningRed = nFGCropStartRed;
                                else
                                    nFGLineBeginningRed = ((int)((float)nFGCropStartRed / cLayerInfo.nWidth_4)) * cLayerInfo.nWidth_4;
                                // -----------		

                                if (nLeftPxIndx >= nFGLineBeginningRed) // левый пиксель ещё в нашей строке
                                {
                                    if (_cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3] == 0) // сложение с пустым пикселем несет опасность появления (проверено) чёрного цвета из него.
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2];
                                    }
                                    else if (_cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3] == 0)
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    }
                                    else
                                    {
                                        nFGColorRed = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx]];
                                        nFGColorGreen = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1]];
                                        nFGColorBlue = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2]];
                                    }
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3]];
                                }
                                else // если наш пиксель первый в строке - он просто "ослабнет"
                                {
                                    nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                    nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                    nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], 0];
                                }
                                #endregion
                            }
                            else if (0 == cLayerInfo.nShiftPositionByteX && 0 != cLayerInfo.nShiftPositionByteY)  // попали не точно в пиксель по Y   (берём с верхнего пикселя) 0 <= S < 255 
                            {
                                #region не точно по Y
                                // ex cLLI init
                                bRowUpper = false;
                                if (nBGIndxLine - 1 >= cLayerInfo.nCropTopLineInBG && nBGIndxLine - 1 <= cLayerInfo.nCropBottomLineInBG)
                                    bRowUpper = true;
                                // -----------

                                if (bRowUpper)  // в нашем FG есть строка выше текущей, которая входит в кроп по BG 
                                {
                                    nUpPxIndx = nFGIndxRed - cLayerInfo.nWidth_4;
                                    if (_cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3] == 0) // сложение с пустым пикселем несет опасность появления (проверено) чёрного цвета из него.
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 2];
                                    }
                                    else if (_cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 3] == 0)
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    }
                                    else
                                    {
                                        nFGColorRed = _aAlphaMap[cLayerInfo.nShiftPositionByteY - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed], _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx]];
                                        nFGColorGreen = _aAlphaMap[cLayerInfo.nShiftPositionByteY - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1], _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 1]];
                                        nFGColorBlue = _aAlphaMap[cLayerInfo.nShiftPositionByteY - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2], _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 2]];
                                    }
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nShiftPositionByteY - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], _cDisComProcessing._aLayers[nLayerIndx][nUpPxIndx + 3]];
                                }
                                else
                                {
                                    nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                    nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                    nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nShiftPositionByteY - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], 0];
                                }
                                #endregion
                            }
                            else //  попали не точно в пиксель и по Х и по Y
                            {
                                bElse = true;
                                // не реализовано! не нужно пока. Но описание вот:
                                // обычно берем инфу с двух соседних пикселей в пропорции шифта - с этого и с пикселя слева от этого. Т.к. был перелет из-за отбрасывания дробной части от X (= x-floor(x))  (floor is  1.2=1 or  -1.2=-2)
                                //  this_pixel = this_pixel * (1-shift)  + left_pixel * shift    тут   0<=shift<1    или =(1-s)T+sL  или  T + s(L-T)
                                // формула в _aAlphaMap   = BG+a(FG-BG)  т.е.  = amap[sbyte, BG, FG]  т.е. та же формула.
                                // формула для диагонального смещения пикселя: (T+sx(L-T))  +  sy(  (U+sx(D-U)) - (T+sx(L-T))  ), где T - this_pixel, U - up_pixel, L - left_pixel, D - diagonal_pixel (Left-Up), sx - смещение по х, sy - смещение по y
                            }
                        }
                        else // невозможный вариант, но ))
                        {
                            bElse = true;
                        }
                    }
                    else // нужны поля. реализовано только для горизонтального движения. для диагональных - см. подсказки здесь и выше.
                    {
                        #region даём поля + неточное попадание пикселей в рамках полей
                        // ex cLLI init
                        if (nBGIndxPixelStartRed < nBGCropStartRed)
                            nFGLineBeginningRed = nFGCropStartRed;
                        else
                            nFGLineBeginningRed = ((int)((float)nFGCropStartRed / cLayerInfo.nWidth_4)) * cLayerInfo.nWidth_4;
                        nBgFgLinesDelta = nBGIndxLine - cLayerInfo.nCropTopLineInBG;
                        // ----------

                        nRowBeginingIndx = nFGLineBeginningRed;
                        if (0 == ((nBgFgLinesDelta + cLayerInfo.nOffsetTop) & 1))   // -----в dvPal это та по чётности строка, которая первой должна показывааться! Т.е. половина движения
                        {
                            nFGIndxRed = nFGIndxRed + cLayerInfo.nHalfDeltaPxX_4; // для влево nHalfDeltaPxX_4  <0  для вправо >0  . Для диагональных движений надо еще и DeltaY*With прибавлять...
                            nLeftPxIndx = nFGIndxRed - 4;
                            nRowEndingIndx = nRowBeginingIndx + cLayerInfo.nWidth_4 - 4;
                            if (nFGIndxRed < nRowBeginingIndx || nLeftPxIndx > nRowEndingIndx || (nFGIndxRed > nRowEndingIndx && 0 == cLayerInfo.nHalfPathShiftPositionByteX))
                            {
                                nFGColorRed = 0;
                                nFGColorGreen = 0;
                                nFGColorBlue = 0;
                                nFGColorAlpha = 0;
                            }
                            else if (0 != cLayerInfo.nHalfPathShiftPositionByteX) //этот вариант не может быть вычислен в _aAlphaMap
                            {
                                // для движения влево  cLayerInfo.nHalfPathShiftPositionByte s>=0       // левый пиксель ещё в нашей строке и наш пиксель тоже
                                if (nLeftPxIndx >= nRowBeginingIndx && nFGIndxRed <= nRowEndingIndx)      //  a+s(a-b)  === amap[sbyte, a, b] , если -1<s<0, 0<sbyte<255 !!
                                {
                                    if (_cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3] == 0) // сложение с пустым пикселем несет опасность появления (проверено) чёрного цвета из него.
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2];
                                    }
                                    else if (_cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3] == 0)
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    }
                                    else
                                    {
                                        nFGColorRed = _aAlphaMap[cLayerInfo.nHalfPathShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx]];
                                        nFGColorGreen = _aAlphaMap[cLayerInfo.nHalfPathShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1]];
                                        nFGColorBlue = _aAlphaMap[cLayerInfo.nHalfPathShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2]];
                                    }
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nHalfPathShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3]];
                                }
                                else if (nLeftPxIndx < nRowBeginingIndx) // только мы в строке
                                {
                                    nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                    nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                    nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nHalfPathShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], 0];
                                }
                                else if (nFGIndxRed > nRowEndingIndx)  // только левый в строке
                                {
                                    nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx];
                                    nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1];
                                    nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2];
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nHalfPathShiftPositionByteX - 1, 0, _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3]];
                                }
                                else // невозможный вариант, но ))
                                {
                                    bElse = true;
                                }
                            }
                            else
                            {
                                nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                nFGColorAlpha = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3];
                            }
                        }
                        else if (bMainCondition)    // -----в dvPal это та по чётности строка, которая второй должна показывааться! Т.е. целое движение
                        {
                            if (0 != cLayerInfo.nShiftPositionByteX)
                            {
                                nLeftPxIndx = nFGIndxRed - 4;
                                if (nLeftPxIndx >= nRowBeginingIndx) // левый пиксель ещё в нашей строке
                                {
                                    if (_cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3] == 0) // сложение с пустым пикселем несет опасность появления (проверено) чёрного цвета из него.
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2];
                                    }
                                    else if (_cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3] == 0)
                                    {
                                        nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                        nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                        nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    }
                                    else
                                    {
                                        nFGColorRed = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx]];
                                        nFGColorGreen = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 1]];
                                        nFGColorBlue = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 2]];
                                    }
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], _cDisComProcessing._aLayers[nLayerIndx][nLeftPxIndx + 3]];
                                }
                                else // если наш пиксель первый в строке - он просто "ослабнет"
                                {
                                    nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                                    nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                                    nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                                    nFGColorAlpha = _aAlphaMap[cLayerInfo.nShiftPositionByteX - 1, _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3], 0];
                                }
                            }
                            else
                            {
                                bElse = true;
                                // else - присвоение в элзе было уже выше этого всего блока - просто обычный пиксель без шифта
                            }
                        }
                        else
                        {
                            nFGColorRed = 0;
                            nFGColorGreen = 0;
                            nFGColorBlue = 0;
                            nFGColorAlpha = 0;
                        }
                        #endregion
                    }
                    #endregion

                    if (bElse)
                    {
                        if (bMainCondition)
                        {
                            nFGColorRed = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed];
                            nFGColorGreen = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 1];
                            nFGColorBlue = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 2];
                            nFGColorAlpha = _cDisComProcessing._aLayers[nLayerIndx][nFGIndxRed + 3];
                        }
                        else
                        {
                            nFGColorRed = 0;
                            nFGColorGreen = 0;
                            nFGColorBlue = 0;
                            nFGColorAlpha = 0;
                        }
                    }

                    nPixelAlpha = cLayerInfo.nAlphaConstant;

                    if (255 == nPixelAlpha)
                        nPixelAlpha = nFGColorAlpha;
                    else if (0 == nFGColorAlpha)
                        nPixelAlpha = 0;
                    else if (0 < nPixelAlpha && 255 > nFGColorAlpha) // объединение альфы слоя с константной альфой !!!!
                        nPixelAlpha = _aAlphaMap2[nFGColorAlpha - 1, nPixelAlpha - 1];  //    (byte)((float)nFGColorAlpha * nPixelAlpha / 255 + 0.5);
                                                                                        //}
                                                                                        //else
                                                                                        //	nPixelAlpha = 255;

                    if (0 < nPixelAlpha && 0 < nExtMaskAlpha)
                        nPixelAlpha = _aAlphaMap3[nPixelAlpha - 1, nExtMaskAlpha - 1];   //(byte)(nPixelAlpha * (1 - nExtMaskAlpha / 255f) + 0.5);   [1--255;1--254(-1)]

                    if (0 < nPixelAlpha)
                    {
                        nBGIndxAlpha = nBGIndxRed + 3;
                        if (255 == nPixelAlpha || 0 == _cDisComProcessing._aLayers[0][nBGIndxAlpha])
                        {
                            _cDisComProcessing._aLayers[0][nBGIndxRed] = nFGColorRed;
                            _cDisComProcessing._aLayers[0][nBGIndxRed + 1] = nFGColorGreen;
                            _cDisComProcessing._aLayers[0][nBGIndxRed + 2] = nFGColorBlue;
                        }
                        else
                        {                           //индекс меньше, т.к. 0-е значение альфы мы не считаем и все индексы сдвинулись...
                            nPixelAlphaIndx = nPixelAlpha - 1;
                            if (!bBGChanged)
                            {
                                _cDisComProcessing._aLayers[0][nBGIndxRed] = _aAlphaMap[nPixelAlphaIndx, 0, nFGColorRed];
                                _cDisComProcessing._aLayers[0][nBGIndxRed + 1] = _aAlphaMap[nPixelAlphaIndx, 0, nFGColorGreen];
                                _cDisComProcessing._aLayers[0][nBGIndxRed + 2] = _aAlphaMap[nPixelAlphaIndx, 0, nFGColorBlue];
                            }
                            else
                            {
                                nBGIndxGreen = nBGIndxRed + 1;    //НА САМОМ ДЕЛЕ - это  BGRA , а не RGBA ))
                                nBGIndxBlue = nBGIndxRed + 2;
                                //pLayers[0][nBGIndxRed] = 255 < (nResult = (nPixelAlpha * (nFGColorRed - pLayers[0][nBGIndxRed])) / 255.0 + pLayers[0][nBGIndxRed] + 0.5) ? 255: (byte)nResult;   при большой нагрузке побеждают массивы - 13сек чистая математика, 12сек только альфа на массивах, 11сек всё на массивах. при маленькой - разница почти не видна.
                                _cDisComProcessing._aLayers[0][nBGIndxRed] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxRed], nFGColorRed];
                                _cDisComProcessing._aLayers[0][nBGIndxGreen] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxGreen], nFGColorGreen];
                                _cDisComProcessing._aLayers[0][nBGIndxBlue] = _aAlphaMap[nPixelAlphaIndx, _cDisComProcessing._aLayers[0][nBGIndxBlue], nFGColorBlue];
                            }
                        }
                        // цвета надо тушить, как выше сделано, если это всё на жестком заднике мы делаем (0,0,0,255) и альфу не трогать, а если это в воздухе всё (0,0,0,0) , то, как ниже, просто альфу надо положить!
                        if (!bBGAChanged || _cDisComProcessing._aLayers[0][nBGIndxAlpha] < nPixelAlpha)   // очередная попытка примирить альфу с действительностью ))   
                        {
                            _cDisComProcessing._aLayers[0][nBGIndxAlpha] = nPixelAlpha;
                            bBGAChanged = true;
                        }
                        bBGChanged = true;
                    }
                }
                nExtMaskAlpha = 0;   // полностью не маскируем слой
            }
            if (!bBGChanged)
            {
                _cDisComProcessing._aLayers[0][nBGIndxRed] = 0;
                _cDisComProcessing._aLayers[0][nBGIndxRed + 1] = 0;
                _cDisComProcessing._aLayers[0][nBGIndxRed + 2] = 0;
            }
            if (!bBGAChanged)
                _cDisComProcessing._aLayers[0][nBGIndxRed + 3] = 0;
        }
    }
}