import { Component, OnInit } from '@angular/core';
import { SharedService } from 'src/app/shared.service';

@Component({
  selector: 'app-inspections-table',
  templateUrl: './inspections-table.component.html',
  styleUrls: ['./inspections-table.component.css']
})
export class InspectionsTableComponent implements OnInit {

  readonly APIUrl = "https://localhost:7023/";

  EquipmentList: any =[];

  constructor(public service: SharedService) { }
  

  ngOnInit(): void {
    this.GetEquipmentList("fire extinguisher");
  }

  GetEquipmentList(equipmentId: any) {
    this.service.GetEquipmentList(equipmentId).subscribe(data => {
      this.EquipmentList = data;
      console.log(this.EquipmentList);
    })
  }

}
