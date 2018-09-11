cbuffer CellGridHelperBuffer: register( b0 ){
    float CellSize;
    float InvCellSize;
};

int3 Floor(float3 p){
    return (int3)(p * InvCellSize + 32768.1) - 32768;
}