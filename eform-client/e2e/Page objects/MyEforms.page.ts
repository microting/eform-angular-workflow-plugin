// noinspection JSIgnoredPromiseFromCall

import {PageWithNavbarPage} from './PageWithNavbar.page';
import { $ } from '@wdio/globals';

class MyEformsPage extends PageWithNavbarPage {
  constructor() {
    super();
  }
}

const myEformsPage = new MyEformsPage();
export default myEformsPage;
