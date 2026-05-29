package ru.neomgtu.proxyrasp.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Lob;
import jakarta.persistence.Table;
import jakarta.persistence.UniqueConstraint;

import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@Entity
@Table(name = "subjects", uniqueConstraints = @UniqueConstraint(columnNames = {"auditorium", "date", "begin_lesson"}))
public class Subject {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String auditorium;

    @Column(name = "begin_lesson")
    private String beginLesson;
    private String building;
    private String createddate;
    private String date;
    private String dayOfWeek;
    private String dayOfWeekString;
    private String discipline;
    private String endLesson;
    private String kindOfWork;
    private String lecturer;
    private String modifieddate;
    private String stream;
    private String subGroup;

    @Lob
    @Column(columnDefinition = "text")
    private String listGroups;
}
