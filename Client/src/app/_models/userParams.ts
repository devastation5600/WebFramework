import { User } from "./users";

export class Userparams {
  gender : string;
  minAge = 18;
  maxAge = 99;
  pageNumber = 1;
  pageSize = 2;
  orderBy = 'lastActive'


  constructor(user: User) {
    this.gender = user.gender === 'female' ? 'male' : 'female';
  }
}
