namespace PositionBasedDynamics.ShaderHelpers{
    public static class ShaderHelper{
        public static int GetNumberOfDispatchGroups(int allCount, int blockSize){
            
            if(allCount % blockSize == 0){
                return allCount/blockSize;
            }

            return allCount/blockSize + 1;


        }

    }
}