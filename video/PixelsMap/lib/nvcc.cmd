rem ECHO OFF
SET TF="c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\TF.exe"
SET EXIT=1
FOR %%i IN ("C:\projects\!helpers\video\PixelsMap\CUDAFunctions.cu") DO For /F "tokens=1,2,3,4,5 delims=.:/ " %%A in ("%%~ti") do SET DATE_SOURCE=%%C%%B%%A%%D%%E
FOR %%i IN ("C:\projects\!helpers\video\PixelsMap\Properties\Resources\CUDAFunctions.20.x64.cubin") DO For /F "tokens=1,2,3,4,5 delims=.:/ " %%A in ("%%~ti") do SET DATE_TARGET=%%C%%B%%A%%D%%E
echo DATE_TARGET=%DATE_TARGET% 
echo DATE_SOURCE=%DATE_SOURCE% 
IF "%DATE_SOURCE%" GTR "%DATE_TARGET%" SET EXIT=0
IF 1==%EXIT% exit

SET NVCC="c:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v6.5\bin\\nvcc.exe"  --cl-version 2010 --cubin %1CUDAFunctions.cu -ccbin "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\bin" -I "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\include"

echo checkout cubins
%TF% checkout "%1Properties\Resources\*.cubin"

echo compiling cubins
%NVCC% -m64 -arch=sm_11 -o "%1Properties\Resources\CUDAFunctions.11.x64.cubin"
%NVCC% -m32 -arch=sm_11 -o "%1Properties\Resources\CUDAFunctions.11.x32.cubin"
%NVCC% -m64 -arch=sm_12 -o "%1Properties\Resources\CUDAFunctions.12.x64.cubin"
%NVCC% -m32 -arch=sm_12 -o "%1Properties\Resources\CUDAFunctions.12.x32.cubin"
%NVCC% -m64 -arch=sm_13 -o "%1Properties\Resources\CUDAFunctions.13.x64.cubin"
%NVCC% -m32 -arch=sm_13 -o "%1Properties\Resources\CUDAFunctions.13.x32.cubin"
%NVCC% -m64 -arch=sm_20 -o "%1Properties\Resources\CUDAFunctions.20.x64.cubin"
%NVCC% -m32 -arch=sm_20 -o "%1Properties\Resources\CUDAFunctions.20.x32.cubin"
%NVCC% -m64 -arch=sm_21 -o "%1Properties\Resources\CUDAFunctions.21.x64.cubin"
%NVCC% -m32 -arch=sm_21 -o "%1Properties\Resources\CUDAFunctions.21.x32.cubin"
%NVCC% -m64 -arch=sm_30 -o "%1Properties\Resources\CUDAFunctions.30.x64.cubin"
%NVCC% -m32 -arch=sm_30 -o "%1Properties\Resources\CUDAFunctions.30.x32.cubin"
%NVCC% -m64 -arch=sm_35 -o "%1Properties\Resources\CUDAFunctions.35.x64.cubin"
%NVCC% -m32 -arch=sm_35 -o "%1Properties\Resources\CUDAFunctions.35.x32.cubin"
%NVCC% -m64 -arch=sm_50 -o "%1Properties\Resources\CUDAFunctions.50.x64.cubin"
%NVCC% -m32 -arch=sm_50 -o "%1Properties\Resources\CUDAFunctions.50.x32.cubin"
