struct Particle{
	uint number;
	int3 cell;
};

inline bool CompareParticle(Particle left, Particle right) {
	//return (left.x == right.x) ? (left.y <= right.y) : (left.x <= right.x);
	//return left <= right;

	if(left.cell.x != right.cell.x){
		return left.cell.x <= right.cell.x;
	}

	if(left.cell.y != right.cell.y){
		return left.cell.y <= right.cell.y;
	}

	return left.cell.z <= right.cell.z;
}


inline int CompareInt3(int3 left, int3 right) {
	//return (left.x == right.x) ? (left.y <= right.y) : (left.x <= right.x);
	//return left <= right;

	if(left.x != right.x){
		return left.x - right.x;
	}

	if(left.y != right.y){
		return left.y - right.y;
	}

	return left.z - right.z;
}
